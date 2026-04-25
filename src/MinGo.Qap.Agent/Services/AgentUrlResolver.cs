using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using MinGo.Qap.Agent.Configuration;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent URL 解析器 - 多环境自适应地址检测
/// </summary>
public class AgentUrlResolver
{
    private readonly ILogger<AgentUrlResolver> _logger;

    public AgentUrlResolver(ILogger<AgentUrlResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 解析 Agent 的外部可访问 URL
    /// </summary>
    /// <param name="settings">Agent 设置</param>
    /// <returns>完整的 HTTP URL</returns>
    public string Resolve(AgentSettings settings)
    {
        // 优先级 1: 显式配置
        if (!string.IsNullOrWhiteSpace(settings.ExternalUrl))
        {
            _logger.LogInformation("Using explicit ExternalUrl: {Url}", settings.ExternalUrl);
            return settings.ExternalUrl;
        }

        // 优先级 2: 环境变量
        var envUrl = Environment.GetEnvironmentVariable("AGENT_URL");
        if (!string.IsNullOrWhiteSpace(envUrl))
        {
            _logger.LogInformation("Using AGENT_URL environment variable: {Url}", envUrl);
            return envUrl;
        }

        // 优先级 3: Kubernetes
        var k8sUrl = ResolveKubernetesUrl(settings.Port);
        if (!string.IsNullOrWhiteSpace(k8sUrl))
        {
            _logger.LogInformation("Using Kubernetes detected URL: {Url}", k8sUrl);
            return k8sUrl;
        }

        // 优先级 4: Docker
        var dockerUrl = ResolveDockerUrl(settings.Port);
        if (!string.IsNullOrWhiteSpace(dockerUrl))
        {
            _logger.LogInformation("Using Docker detected URL: {Url}", dockerUrl);
            return dockerUrl;
        }

        // 优先级 5: 本地自动检测
        var localUrl = ResolveLocalUrl(settings.Port, settings.NetworkInterface);
        _logger.LogInformation("Using local detected URL: {Url}", localUrl);
        return localUrl;
    }

    private string? ResolveKubernetesUrl(int port)
    {
        try
        {
            // K8s Downward API: POD_IP
            var podIp = Environment.GetEnvironmentVariable("POD_IP");
            if (!string.IsNullOrWhiteSpace(podIp))
            {
                return $"http://{podIp}:{port}";
            }

            // K8s Service 环境变量存在
            var k8sHost = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
            if (!string.IsNullOrWhiteSpace(k8sHost))
            {
                var hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? Dns.GetHostName();
                return $"http://{hostname}:{port}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Kubernetes URL detection failed");
        }

        return null;
    }

    private string? ResolveDockerUrl(int port)
    {
        try
        {
            // 检查是否在 Docker 容器中
            if (!File.Exists("/.dockerenv"))
            {
                return null;
            }

            // 尝试获取容器 IP
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ip = hostEntry.AddressList
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork
                                     && !IPAddress.IsLoopback(a))
                ?.ToString();

            if (!string.IsNullOrWhiteSpace(ip))
            {
                return $"http://{ip}:{port}";
            }

            // 回退到 HOSTNAME
            var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
            if (!string.IsNullOrWhiteSpace(hostname))
            {
                return $"http://{hostname}:{port}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Docker URL detection failed");
        }

        return null;
    }

    private string ResolveLocalUrl(int port, string? networkInterface)
    {
        try
        {
            // 指定网卡
            if (!string.IsNullOrWhiteSpace(networkInterface) && networkInterface != "0.0.0.0")
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n =>
                        string.Equals(n.Name, networkInterface, StringComparison.OrdinalIgnoreCase));

                if (ni != null)
                {
                    var ip = ni.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork
                                             && !IPAddress.IsLoopback(a.Address))
                        ?.Address.ToString();

                    if (!string.IsNullOrWhiteSpace(ip))
                    {
                        return $"http://{ip}:{port}";
                    }
                }

                _logger.LogWarning("Network interface '{Interface}' not found or has no IPv4 address", networkInterface);
            }

            // 尝试获取第一个非回环 IPv4 地址
            var localIp = GetLocalIpAddress();
            if (!string.IsNullOrWhiteSpace(localIp))
            {
                return $"http://{localIp}:{port}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Local URL detection failed");
        }

        // 最终回退
        var machineName = Environment.MachineName.ToLowerInvariant();
        return $"http://{machineName}:{port}";
    }

    /// <summary>
    /// 获取本地非回环 IPv4 地址
    /// </summary>
    private static string? GetLocalIpAddress()
    {
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var props = ni.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork
                        && !IPAddress.IsLoopback(addr.Address))
                    {
                        return addr.Address.ToString();
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }
}
