import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Select,
  Input,
  message,
  Tag,
  Space,
  Badge,
  Alert,
  Statistic,
  Row,
  Col,
} from 'antd';
import {
  WarningOutlined,
  EyeOutlined,
  EditOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import {
  getAdminDeliveryMonitor,
  getAdminDeliveryTimeline,
  adminOverrideDeliveryStatus,
  EVENT_DELIVERY_STATUS,
  getEventDeliveryStatusText,
} from '../../services/deliveryApi';
import dayjs from 'dayjs';

const { Option } = Select;
const { TextArea } = Input;

/**
 * Admin Delivery Monitor Component
 * Allows admins to monitor all deliveries and override status when needed
 */
const DeliveryMonitor = () => {
  const [loading, setLoading] = useState(false);
  const [deliveries, setDeliveries] = useState([]);
  const [selectedDelivery, setSelectedDelivery] = useState(null);
  const [timelineVisible, setTimelineVisible] = useState(false);
  const [overrideVisible, setOverrideVisible] = useState(false);
  const [timeline, setTimeline] = useState(null);
  const [form] = Form.useForm();

  useEffect(() => {
    fetchDeliveries();
    // Auto-refresh every 60 seconds
    const interval = setInterval(fetchDeliveries, 60000);
    return () => clearInterval(interval);
  }, []);

  const fetchDeliveries = async () => {
    try {
      setLoading(true);
      const response = await getAdminDeliveryMonitor();

      if (response.success) {
        setDeliveries(response.data || []);
      } else {
        message.error(response.message || 'Failed to load deliveries');
      }
    } catch (error) {
      console.error('Error fetching deliveries:', error);
      message.error('Failed to load deliveries. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleViewTimeline = async (delivery) => {
    try {
      setSelectedDelivery(delivery);
      setTimelineVisible(true);

      const response = await getAdminDeliveryTimeline(delivery.orderId);

      if (response.success) {
        setTimeline(response.data);
      } else {
        message.error('Failed to load timeline');
      }
    } catch (error) {
      console.error('Error fetching timeline:', error);
      message.error('Failed to load timeline');
    }
  };

  const handleOverrideStatus = (delivery) => {
    setSelectedDelivery(delivery);
    setOverrideVisible(true);
    form.setFieldsValue({
      newStatus: delivery.currentStatus,
    });
  };

  const handleOverrideSubmit = async (values) => {
    try {
      const overrideData = {
        eventDeliveryId: selectedDelivery.eventDeliveryId,
        newStatus: values.newStatus,
        notes: values.notes,
      };

      const response = await adminOverrideDeliveryStatus(overrideData);

      if (response.success) {
        message.success('Delivery status overridden successfully');
        setOverrideVisible(false);
        form.resetFields();
        fetchDeliveries();
      } else {
        message.error(response.message || 'Failed to override status');
      }
    } catch (error) {
      console.error('Error overriding status:', error);
      message.error('Failed to override status. Please try again.');
    }
  };

  const getStatusColor = (status) => {
    const colorMap = {
      [EVENT_DELIVERY_STATUS.PREPARATION_STARTED]: 'blue',
      [EVENT_DELIVERY_STATUS.VEHICLE_READY]: 'cyan',
      [EVENT_DELIVERY_STATUS.DISPATCHED]: 'orange',
      [EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE]: 'purple',
      [EVENT_DELIVERY_STATUS.EVENT_COMPLETED]: 'green',
    };
    return colorMap[status] || 'default';
  };

  const getStatistics = () => {
    const total = deliveries.length;
    const delayed = deliveries.filter((d) => d.isDelayed).length;
    const dispatched = deliveries.filter(
      (d) => d.currentStatus === EVENT_DELIVERY_STATUS.DISPATCHED
    ).length;
    const completed = deliveries.filter(
      (d) => d.currentStatus === EVENT_DELIVERY_STATUS.EVENT_COMPLETED
    ).length;

    return { total, delayed, dispatched, completed };
  };

  const stats = getStatistics();

  const columns = [
    {
      title: 'Order ID',
      dataIndex: 'orderId',
      key: 'orderId',
      width: 100,
      sorter: (a, b) => a.orderId - b.orderId,
    },
    {
      title: 'Partner',
      dataIndex: 'ownerBusinessName',
      key: 'ownerBusinessName',
      ellipsis: true,
    },
    {
      title: 'Status',
      dataIndex: 'currentStatus',
      key: 'currentStatus',
      render: (status, record) => (
        <Space>
          <Tag color={getStatusColor(status)}>{getEventDeliveryStatusText(status)}</Tag>
          {record.isDelayed && (
            <Badge count={<WarningOutlined style={{ color: '#f5222d' }} />} />
          )}
        </Space>
      ),
      filters: [
        { text: 'Preparation Started', value: EVENT_DELIVERY_STATUS.PREPARATION_STARTED },
        { text: 'Vehicle Ready', value: EVENT_DELIVERY_STATUS.VEHICLE_READY },
        { text: 'Dispatched', value: EVENT_DELIVERY_STATUS.DISPATCHED },
        { text: 'Arrived at Venue', value: EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE },
        { text: 'Event Completed', value: EVENT_DELIVERY_STATUS.EVENT_COMPLETED },
      ],
      onFilter: (value, record) => record.currentStatus === value,
    },
    {
      title: 'Vehicle',
      dataIndex: 'vehicleNumber',
      key: 'vehicleNumber',
      render: (text) => text || '-',
    },
    {
      title: 'Event Date',
      dataIndex: 'eventDate',
      key: 'eventDate',
      render: (date) => dayjs(date).format('DD MMM YYYY'),
      sorter: (a, b) => dayjs(a.eventDate).unix() - dayjs(b.eventDate).unix(),
    },
    {
      title: 'Scheduled Dispatch',
      dataIndex: 'scheduledDispatchTime',
      key: 'scheduledDispatchTime',
      render: (time) => (time ? dayjs(time).format('DD MMM, hh:mm A') : '-'),
    },
    {
      title: 'Delay',
      dataIndex: 'isDelayed',
      key: 'isDelayed',
      render: (isDelayed, record) =>
        isDelayed ? (
          <Tag color="red">{record.delayMinutes} min</Tag>
        ) : (
          <Tag color="green">On Time</Tag>
        ),
      sorter: (a, b) => (a.delayMinutes || 0) - (b.delayMinutes || 0),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button size="small" icon={<EyeOutlined />} onClick={() => handleViewTimeline(record)}>
            Timeline
          </Button>
          <Button
            size="small"
            icon={<EditOutlined />}
            danger
            onClick={() => handleOverrideStatus(record)}
          >
            Override
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div>
      {/* Statistics Cards */}
      <Row gutter={16} style={{ marginBottom: '16px' }}>
        <Col span={6}>
          <Card>
            <Statistic title="Total Active Deliveries" value={stats.total} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Delayed"
              value={stats.delayed}
              valueStyle={{ color: stats.delayed > 0 ? '#cf1322' : '#3f8600' }}
              prefix={stats.delayed > 0 ? <WarningOutlined /> : null}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic title="In Transit" value={stats.dispatched} valueStyle={{ color: '#1890ff' }} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic title="Completed Today" value={stats.completed} valueStyle={{ color: '#3f8600' }} />
          </Card>
        </Col>
      </Row>

      {/* Main Table */}
      <Card
        title="Delivery Monitor"
        extra={
          <Button icon={<ReloadOutlined />} onClick={fetchDeliveries}>
            Refresh
          </Button>
        }
      >
        <Alert
          message="Admin Monitoring Dashboard"
          description="Monitor all active deliveries. Use override functionality only when necessary. Auto-refreshes every 60 seconds."
          type="info"
          showIcon
          style={{ marginBottom: '16px' }}
        />

        <Table
          columns={columns}
          dataSource={deliveries}
          loading={loading}
          rowKey="orderId"
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} deliveries`,
          }}
          rowClassName={(record) => (record.isDelayed ? 'delayed-row' : '')}
        />
      </Card>

      {/* Timeline Modal */}
      <Modal
        title={`Delivery Timeline - Order #${selectedDelivery?.orderId}`}
        open={timelineVisible}
        onCancel={() => {
          setTimelineVisible(false);
          setSelectedDelivery(null);
          setTimeline(null);
        }}
        footer={null}
        width={600}
      >
        {timeline && (
          <div>
            <Alert
              message={timeline.currentStatusText}
              type="info"
              showIcon
              style={{ marginBottom: '16px' }}
            />

            {timeline.statusHistory.map((item, index) => (
              <div key={index} style={{ marginBottom: '12px', padding: '12px', backgroundColor: '#f5f5f5', borderRadius: '4px' }}>
                <div>
                  <Tag color={getStatusColor(item.newStatus)}>
                    {getEventDeliveryStatusText(item.newStatus)}
                  </Tag>
                  <span style={{ fontSize: '12px', color: '#888', marginLeft: '8px' }}>
                    {dayjs(item.changedAt).format('DD MMM YYYY, hh:mm A')}
                  </span>
                </div>
                {item.changedByType && (
                  <div style={{ fontSize: '12px', marginTop: '4px' }}>
                    Changed by: <strong>{item.changedByType}</strong>
                  </div>
                )}
                {item.notes && (
                  <div style={{ fontSize: '12px', fontStyle: 'italic', marginTop: '4px' }}>
                    {item.notes}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </Modal>

      {/* Override Status Modal */}
      <Modal
        title={
          <span style={{ color: '#cf1322' }}>
            <WarningOutlined /> Admin Override - Order #{selectedDelivery?.orderId}
          </span>
        }
        open={overrideVisible}
        onCancel={() => {
          setOverrideVisible(false);
          form.resetFields();
        }}
        onOk={() => form.submit()}
        okText="Override Status"
        okButtonProps={{ danger: true }}
      >
        <Alert
          message="Warning: Admin Override"
          description="This action will override the current delivery status. Use with caution. The change will be logged."
          type="warning"
          showIcon
          style={{ marginBottom: '16px' }}
        />

        <Form form={form} layout="vertical" onFinish={handleOverrideSubmit}>
          <Form.Item
            label="New Status"
            name="newStatus"
            rules={[{ required: true, message: 'Please select new status' }]}
          >
            <Select placeholder="Select new status">
              <Option value={EVENT_DELIVERY_STATUS.PREPARATION_STARTED}>
                Preparation Started
              </Option>
              <Option value={EVENT_DELIVERY_STATUS.VEHICLE_READY}>Vehicle Ready</Option>
              <Option value={EVENT_DELIVERY_STATUS.DISPATCHED}>Dispatched</Option>
              <Option value={EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE}>
                Arrived at Venue
              </Option>
              <Option value={EVENT_DELIVERY_STATUS.EVENT_COMPLETED}>
                Event Completed
              </Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="Reason for Override"
            name="notes"
            rules={[{ required: true, message: 'Please provide a reason' }]}
          >
            <TextArea
              rows={4}
              placeholder="Explain why you're overriding the status (required for audit)"
            />
          </Form.Item>
        </Form>
      </Modal>

      <style jsx>{`
        .delayed-row {
          background-color: #fff1f0;
        }
      `}</style>
    </div>
  );
};

export default DeliveryMonitor;
