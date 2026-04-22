import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

const ClusterDetailPage: React.FC = () => {
  const { clusterId } = useParams<{ clusterId: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    if (clusterId) {
      navigate(`/clusters/${clusterId}`, { replace: true });
    }
  }, [clusterId, navigate]);

  return null;
};

export default ClusterDetailPage;