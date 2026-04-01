import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Input,
  DatePicker,
  message,
  Tag,
  Space,
  Divider,
  Timeline,
  Alert,
} from 'antd';
import {
  CheckCircleOutlined,
  RightOutlined,
  EyeOutlined,
  CarOutlined,
} from '@ant-design/icons';
import {
  getPartnerActiveDeliveries,
  updateEventDeliveryStatus,
  getPartnerEventDeliveryTimeline,
  initEventDelivery,
  EVENT_DELIVERY_STATUS,
  getEventDeliveryStatusText,
} from '../../../services/deliveryApi';
import dayjs from 'dayjs';

/**
 * Partner Event Delivery Management Component
 * Allows partners to manage and update delivery status for their orders
 */
const EventDeliveryManagement = () => {
  const [loading, setLoading] = useState(false);
  const [deliveries, setDeliveries] = useState([]);
  const [selectedDelivery, setSelectedDelivery] = useState(null);
  const [timelineVisible, setTimelineVisible] = useState(false);
  const [timeline, setTimeline] = useState(null);
  const [initModalVisible, setInitModalVisible] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    fetchActiveDeliveries();
  }, []);

  const fetchActiveDeliveries = async () => {
    try {
      setLoading(true);
      const response = await getPartnerActiveDeliveries();

      if (response.success) {
        setDeliveries(response.data || []);
      } else {
        message.error(response.message || 'Failed to load deliveries');
      }
    } catch (error) {
      console.error('Error fetching active deliveries:', error);
      message.error('Failed to load deliveries. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (delivery, newStatus) => {
    try {
      const statusData = {
        eventDeliveryId: delivery.eventDeliveryId,
        newStatus: newStatus,
      };

      const response = await updateEventDeliveryStatus(statusData);

      if (response.success) {
        message.success('Delivery status updated successfully');
        fetchActiveDeliveries();
      } else {
        message.error(response.message || 'Failed to update status');
      }
    } catch (error) {
      console.error('Error updating delivery status:', error);
      message.error('Failed to update status. Please try again.');
    }
  };

  const handleViewTimeline = async (delivery) => {
    try {
      setSelectedDelivery(delivery);
      setTimelineVisible(true);

      const response = await getPartnerEventDeliveryTimeline(delivery.orderId);

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

  const handleInitDelivery = async (values) => {
    try {
      const initData = {
        orderId: values.orderId,
        scheduledDispatchTime: values.scheduledDispatchTime?.toISOString(),
        vehicleNumber: values.vehicleNumber,
        driverName: values.driverName,
        driverPhone: values.driverPhone,
      };

      const response = await initEventDelivery(initData);

      if (response.success) {
        message.success('Event delivery initialized successfully');
        setInitModalVisible(false);
        form.resetFields();
        fetchActiveDeliveries();
      } else {
        message.error(response.message || 'Failed to initialize delivery');
      }
    } catch (error) {
      console.error('Error initializing delivery:', error);
      message.error('Failed to initialize delivery. Please try again.');
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

  const getNextAction = (currentStatus) => {
    const actionMap = {
      [EVENT_DELIVERY_STATUS.PREPARATION_STARTED]: {
        nextStatus: EVENT_DELIVERY_STATUS.VEHICLE_READY,
        label: 'Mark Vehicle Ready',
        icon: <CarOutlined />,
      },
      [EVENT_DELIVERY_STATUS.VEHICLE_READY]: {
        nextStatus: EVENT_DELIVERY_STATUS.DISPATCHED,
        label: 'Dispatch Vehicle',
        icon: <RightOutlined />,
      },
      [EVENT_DELIVERY_STATUS.DISPATCHED]: {
        nextStatus: EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE,
        label: 'Mark Arrived',
        icon: <CheckCircleOutlined />,
      },
      [EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE]: {
        nextStatus: EVENT_DELIVERY_STATUS.EVENT_COMPLETED,
        label: 'Complete Event',
        icon: <CheckCircleOutlined />,
      },
    };
    return actionMap[currentStatus] || null;
  };

  const columns = [
    {
      title: 'Order ID',
      dataIndex: 'orderId',
      key: 'orderId',
      width: 120,
    },
    {
      title: 'Current Status',
      dataIndex: 'deliveryStatus',
      key: 'deliveryStatus',
      render: (status) => (
        <Tag color={getStatusColor(status)}>{getEventDeliveryStatusText(status)}</Tag>
      ),
    },
    {
      title: 'Vehicle',
      dataIndex: 'vehicleNumber',
      key: 'vehicleNumber',
      render: (text) => text || '-',
    },
    {
      title: 'Driver',
      dataIndex: 'driverName',
      key: 'driverName',
      render: (text) => text || '-',
    },
    {
      title: 'Scheduled Dispatch',
      dataIndex: 'scheduledDispatchTime',
      key: 'scheduledDispatchTime',
      render: (time) => (time ? dayjs(time).format('DD MMM YYYY, hh:mm A') : '-'),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => {
        const nextAction = getNextAction(record.deliveryStatus);
        return (
          <Space>
            {nextAction && (
              <Button
                type="primary"
                icon={nextAction.icon}
                onClick={() => handleUpdateStatus(record, nextAction.nextStatus)}
              >
                {nextAction.label}
              </Button>
            )}
            <Button icon={<EyeOutlined />} onClick={() => handleViewTimeline(record)}>
              View Timeline
            </Button>
          </Space>
        );
      },
    },
  ];

  return (
    <div>
      <Card
        title="Event Delivery Management"
        extra={
          <Button type="primary" onClick={() => setInitModalVisible(true)}>
            Initialize New Delivery
          </Button>
        }
      >
        <Alert
          message="Important: Status-Based Delivery"
          description="Update delivery status at each milestone. Customers will be notified automatically. Status must follow the sequence: Preparation Started → Vehicle Ready → Dispatched → Arrived → Completed."
          type="info"
          showIcon
          style={{ marginBottom: '16px' }}
        />

        <Table
          columns={columns}
          dataSource={deliveries}
          loading={loading}
          rowKey="eventDeliveryId"
          pagination={false}
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

            <Timeline
              items={timeline.statusHistory.map((item) => ({
                color: getStatusColor(item.newStatus),
                children: (
                  <div>
                    <div>
                      <strong>{getEventDeliveryStatusText(item.newStatus)}</strong>
                    </div>
                    <div style={{ fontSize: '12px', color: '#888' }}>
                      {dayjs(item.changedAt).format('DD MMM YYYY, hh:mm A')}
                    </div>
                    {item.notes && (
                      <div style={{ fontSize: '12px', fontStyle: 'italic', marginTop: '4px' }}>
                        {item.notes}
                      </div>
                    )}
                  </div>
                ),
              }))}
            />
          </div>
        )}
      </Modal>

      {/* Initialize Delivery Modal */}
      <Modal
        title="Initialize Event Delivery"
        open={initModalVisible}
        onCancel={() => {
          setInitModalVisible(false);
          form.resetFields();
        }}
        onOk={() => form.submit()}
        okText="Initialize"
      >
        <Form form={form} layout="vertical" onFinish={handleInitDelivery}>
          <Form.Item
            label="Order ID"
            name="orderId"
            rules={[{ required: true, message: 'Please enter order ID' }]}
          >
            <Input type="number" placeholder="Enter order ID" />
          </Form.Item>

          <Form.Item label="Scheduled Dispatch Time" name="scheduledDispatchTime">
            <DatePicker showTime style={{ width: '100%' }} />
          </Form.Item>

          <Divider />

          <Form.Item label="Vehicle Number" name="vehicleNumber">
            <Input placeholder="Enter vehicle number (optional)" />
          </Form.Item>

          <Form.Item label="Driver Name" name="driverName">
            <Input placeholder="Enter driver name (optional)" />
          </Form.Item>

          <Form.Item label="Driver Phone" name="driverPhone">
            <Input placeholder="Enter driver phone (optional)" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default EventDeliveryManagement;
