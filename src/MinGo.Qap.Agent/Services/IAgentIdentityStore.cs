using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent 身份信息持久化存储接口
/// </summary>
public interface IAgentIdentityStore
{
    /// <summary>
    /// 加载 Agent 身份信息
    /// </summary>
    /// <returns>AgentIdentity，如果不存在则返回 null</returns>
    AgentIdentity? Load();

    /// <summary>
    /// 保存 Agent 身份信息
    /// </summary>
    /// <param name="identity">身份信息</param>
    void Save(AgentIdentity identity);

    /// <summary>
    /// 清除 Agent 身份信息
    /// </summary>
    void Clear();
}
