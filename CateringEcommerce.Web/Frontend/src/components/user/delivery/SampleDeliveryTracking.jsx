import React, { useState, useEffect } from 'react';
import { Card, Button, Alert, Spin, Typography, Tag, Divider, Row, Col } from 'antd';
import {
  EnvironmentOutlined,
  CarOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
} from '@ant-design/icons';
import {
  getSampleDeliveryTracking,
  SAMPLE_DELIVERY_STATUS,
  getSampleDeliveryStatusText,
} from '../../../services/deliveryApi';

const { Title, Text } = Typography;

/**
 * Sample Delivery Tracking Component - Third-party real-time tracking
 * Shows tracking link and current status
 */
const SampleDeliveryTracking = ({ orderId }) => {
  const [loading, setLoading] = useState(true);
  const [delivery, setDelivery] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (orderId) {
      fetchTracking();
      // Auto-refresh every 30 seconds
      const interval = setInterval(fetchTracking, 30000);
      return () => clearInterval(interval);
    }
  }, [orderId]);

  const fetchTracking = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getSampleDeliveryTracking(orderId);

      if (response.success) {
        setDelivery(response.data);
      } else {
        setError(response.message || 'Failed to load tracking information');
      }
    } catch (err) {
      console.error('Error fetching sample delivery tracking:', err);
      setError('Unable to load tracking information. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case SAMPLE_DELIVERY_STATUS.REQUESTED:
        return 'blue';
      case SAMPLE_DELIVERY_STATUS.PICKED_UP:
        return 'cyan';
      case SAMPLE_DELIVERY_STATUS.IN_TRANSIT:
        return 'orange';
      case SAMPLE_DELIVERY_STATUS.DELIVERED:
        return 'green';
      case SAMPLE_DELIVERY_STATUS.FAILED:
        return 'red';
      default:
        return 'default';
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case SAMPLE_DELIVERY_STATUS.DELIVERED:
        return <CheckCircleOutlined />;
      case SAMPLE_DELIVERY_STATUS.FAILED:
        return <CloseCircleOutlined />;
      case SAMPLE_DELIVERY_STATUS.IN_TRANSIT:
        return <CarOutlined />;
      default:
        return <EnvironmentOutlined />;
    }
  };

  const handleTrackDelivery = () => {
    if (delivery?.trackingUrl) {
      window.open(delivery.trackingUrl, '_blank');
    }
  };

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text>Loading tracking information...</Text>
          </div>
        </div>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <Alert message="Error" description={error} type="error" showIcon />
      </Card>
    );
  }

  if (!delivery) {
    return (
      <Card>
        <Alert
          message="Tracking Not Available"
          description="Sample delivery tracking will be available once the order is confirmed and assigned to a delivery partner."
          type="info"
          showIcon
        />
      </Card>
    );
  }

  return (
    <Card title={<Title level={4}>Sample Delivery Tracking</Title>}>
      {/* Current Status */}
      <div style={{ marginBottom: '24px', textAlign: 'center' }}>
        <Tag
          icon={getStatusIcon(delivery.deliveryStatus)}
          color={getStatusColor(delivery.deliveryStatus)}
          style={{ fontSize: '16px', padding: '8px 16px' }}
        >
          {getSampleDeliveryStatusText(delivery.deliveryStatus)}
        </Tag>
      </div>

      <Divider />

      {/* Delivery Provider */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={12}>
          <Card size="small" style={{ backgroundColor: '#f0f2f5' }}>
            <Text type="secondary">Delivery Provider</Text>
            <div>
              <Text strong style={{ fontSize: '16px' }}>
                {delivery.provider || 'N/A'}
              </Text>
            </div>
          </Card>
        </Col>
        <Col span={12}>
          <Card size="small" style={{ backgroundColor: '#f0f2f5' }}>
            <Text type="secondary">Tracking ID</Text>
            <div>
              <Text strong style={{ fontSize: '14px' }} copyable>
                {delivery.trackingId || 'N/A'}
              </Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Track Live Button */}
      {delivery.trackingUrl && (
        <div style={{ textAlign: 'center', marginBottom: '24px' }}>
          <Button
            type="primary"
            size="large"
            icon={<EnvironmentOutlined />}
            onClick={handleTrackDelivery}
            style={{ minWidth: '200px' }}
          >
            Track Live on {delivery.provider}
          </Button>
          <div style={{ marginTop: '8px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Click to open live tracking in a new tab
            </Text>
          </div>
        </div>
      )}

      <Divider />

      {/* Status Information */}
      {delivery.deliveryStatus === SAMPLE_DELIVERY_STATUS.DELIVERED ? (
        <Alert
          message="Delivery Completed"
          description="Your sample has been delivered successfully. Enjoy!"
          type="success"
          showIcon
        />
      ) : delivery.deliveryStatus === SAMPLE_DELIVERY_STATUS.FAILED ? (
        <Alert
          message="Delivery Failed"
          description="Unfortunately, the delivery could not be completed. Please contact support for assistance."
          type="error"
          showIcon
        />
      ) : delivery.deliveryStatus === SAMPLE_DELIVERY_STATUS.IN_TRANSIT ? (
        <Alert
          message="On the Way"
          description="Your sample is currently being delivered. Track live for real-time location updates."
          type="info"
          showIcon
        />
      ) : (
        <Alert
          message="Processing"
          description="Your sample delivery is being processed. You'll be notified once it's on the way."
          type="info"
          showIcon
        />
      )}

      {/* Timestamps */}
      <div style={{ marginTop: '24px' }}>
        <Text type="secondary">Order Placed</Text>
        <div>
          <Text>{new Date(delivery.createdAt).toLocaleString()}</Text>
        </div>
        {delivery.updatedAt && (
          <div style={{ marginTop: '8px' }}>
            <Text type="secondary">Last Updated</Text>
            <div>
              <Text>{new Date(delivery.updatedAt).toLocaleString()}</Text>
            </div>
          </div>
        )}
      </div>

      {/* Refresh Notice */}
      <Alert
        message="Auto-Refresh Enabled"
        description="Tracking information updates automatically every 30 seconds."
        type="info"
        showIcon
        style={{ marginTop: '16px' }}
      />
    </Card>
  );
};

export default SampleDeliveryTracking;
