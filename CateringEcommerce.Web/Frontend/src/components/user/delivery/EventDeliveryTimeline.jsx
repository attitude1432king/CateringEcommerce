import React, { useState, useEffect } from 'react';
import { Card, Timeline, Tag, Row, Col, Typography, Divider, Alert, Spin } from 'antd';
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  LoadingOutlined,
} from '@ant-design/icons';
import { getEventDeliveryTimeline, EVENT_DELIVERY_STATUS } from '../../../services/deliveryApi';

const { Title, Text } = Typography;

/**
 * Event Delivery Timeline Component - Status-based delivery (NO GPS)
 * Shows delivery progress through status milestones
 */
const EventDeliveryTimeline = ({ orderId }) => {
  const [loading, setLoading] = useState(true);
  const [timeline, setTimeline] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (orderId) {
      fetchTimeline();
    }
  }, [orderId]);

  const fetchTimeline = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getEventDeliveryTimeline(orderId);

      if (response.success) {
        setTimeline(response.data);
      } else {
        setError(response.message || 'Failed to load delivery timeline');
      }
    } catch (err) {
      console.error('Error fetching delivery timeline:', err);
      setError('Unable to load delivery information. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getTimelineItems = () => {
    if (!timeline?.eventDelivery) return [];

    const currentStatus = timeline.eventDelivery.deliveryStatus;

    const statusConfig = [
      {
        status: EVENT_DELIVERY_STATUS.PREPARATION_STARTED,
        label: 'Food Preparation Started',
        icon: '🍳',
        color: 'blue',
      },
      {
        status: EVENT_DELIVERY_STATUS.VEHICLE_READY,
        label: 'Vehicle Ready',
        icon: '🚛',
        color: 'cyan',
      },
      {
        status: EVENT_DELIVERY_STATUS.DISPATCHED,
        label: 'Dispatched to Venue',
        icon: '🚚',
        color: 'orange',
      },
      {
        status: EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE,
        label: 'Arrived at Venue',
        icon: '📍',
        color: 'purple',
      },
      {
        status: EVENT_DELIVERY_STATUS.EVENT_COMPLETED,
        label: 'Event Completed',
        icon: '✅',
        color: 'green',
      },
    ];

    return statusConfig.map((config) => {
      const isCompleted = currentStatus >= config.status;
      const isCurrent = currentStatus === config.status;
      const historyItem = timeline.statusHistory.find(
        (h) => h.newStatus === config.status
      );

      return {
        color: isCompleted ? config.color : 'gray',
        dot: isCurrent ? (
          <LoadingOutlined style={{ fontSize: 16 }} spin />
        ) : isCompleted ? (
          <CheckCircleOutlined style={{ fontSize: 16 }} />
        ) : (
          <ClockCircleOutlined style={{ fontSize: 16 }} />
        ),
        children: (
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <span style={{ fontSize: '20px' }}>{config.icon}</span>
              <Text strong={isCurrent} style={{ fontSize: isCurrent ? '16px' : '14px' }}>
                {config.label}
              </Text>
              {isCurrent && (
                <Tag color="processing" style={{ marginLeft: '8px' }}>
                  Current
                </Tag>
              )}
            </div>
            {historyItem && (
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {new Date(historyItem.changedAt).toLocaleString()}
              </Text>
            )}
            {historyItem?.notes && (
              <div style={{ marginTop: '4px' }}>
                <Text italic style={{ fontSize: '12px' }}>
                  {historyItem.notes}
                </Text>
              </div>
            )}
          </div>
        ),
      };
    });
  };

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text>Loading delivery information...</Text>
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

  if (!timeline?.eventDelivery) {
    return (
      <Card>
        <Alert
          message="Delivery Not Started"
          description="Delivery tracking will be available once the partner starts preparing your order."
          type="info"
          showIcon
        />
      </Card>
    );
  }

  const { eventDelivery } = timeline;

  return (
    <Card title={<Title level={4}>Delivery Tracking</Title>}>
      {/* Vehicle & Driver Information */}
      {(eventDelivery.vehicleNumber || eventDelivery.driverName) && (
        <>
          <Row gutter={16} style={{ marginBottom: '24px' }}>
            {eventDelivery.vehicleNumber && (
              <Col span={12}>
                <Card size="small" style={{ backgroundColor: '#f0f2f5' }}>
                  <Text type="secondary">Vehicle Number</Text>
                  <div>
                    <Text strong style={{ fontSize: '16px' }}>
                      {eventDelivery.vehicleNumber}
                    </Text>
                  </div>
                </Card>
              </Col>
            )}
            {eventDelivery.driverName && (
              <Col span={12}>
                <Card size="small" style={{ backgroundColor: '#f0f2f5' }}>
                  <Text type="secondary">Driver Name</Text>
                  <div>
                    <Text strong style={{ fontSize: '16px' }}>
                      {eventDelivery.driverName}
                    </Text>
                  </div>
                </Card>
              </Col>
            )}
          </Row>
          <Divider />
        </>
      )}

      {/* Scheduled Dispatch Time */}
      {eventDelivery.scheduledDispatchTime && (
        <>
          <div style={{ marginBottom: '24px' }}>
            <Text type="secondary">Scheduled Dispatch Time</Text>
            <div>
              <Text strong style={{ fontSize: '16px' }}>
                {new Date(eventDelivery.scheduledDispatchTime).toLocaleString()}
              </Text>
            </div>
          </div>
          <Divider />
        </>
      )}

      {/* Current Status Alert */}
      <Alert
        message={timeline.currentStatusText}
        description="Your order is being processed. You'll be notified at each step."
        type="success"
        showIcon
        style={{ marginBottom: '24px' }}
      />

      {/* Timeline */}
      <Timeline items={getTimelineItems()} />

      {/* Important Notice */}
      <Alert
        message="Status-Based Tracking"
        description="This is status-based tracking. The partner will update the delivery status at each milestone. You will receive notifications for each update."
        type="info"
        showIcon
        style={{ marginTop: '24px' }}
      />
    </Card>
  );
};

export default EventDeliveryTimeline;
