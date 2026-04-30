/**
 * SupervisorTrackingHub - SignalR Client Service
 * Real-time event tracking for supervisors, partners, and admins
 * Connects to: /hubs/supervisor-tracking
 */

import * as signalR from '@microsoft/signalr';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

class SupervisorTrackingService {
    constructor() {
        this.connection = null;
    }

    /**
     * Initialize SignalR connection with JWT auth
     * @param {string} token - JWT access token
     */
    async connect(token) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            return; // Already connected
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE_URL}/hubs/supervisor-tracking`, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Handle reconnection
        this.connection.onreconnecting((error) => {
            console.warn('SupervisorTrackingHub reconnecting:', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('SupervisorTrackingHub reconnected:', connectionId);
        });

        this.connection.onclose((error) => {
            console.error('SupervisorTrackingHub connection closed:', error);
        });

        await this.connection.start();
        console.log('SupervisorTrackingHub connected');
    }

    /**
     * Subscribe to real-time updates for a specific assignment
     * @param {number} assignmentId
     */
    async subscribeToAssignment(assignmentId) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('SubscribeToAssignment', assignmentId);
        }
    }

    /**
     * Unsubscribe from assignment updates
     * @param {number} assignmentId
     */
    async unsubscribeFromAssignment(assignmentId) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('UnsubscribeFromAssignment', assignmentId);
        }
    }

    /** Listen for supervisor check-in events */
    onSupervisorCheckedIn(callback) {
        this.connection?.on('SupervisorCheckedIn', callback);
    }

    /** Listen for event progress updates (guest count, food serving, etc.) */
    onEventProgressUpdate(callback) {
        this.connection?.on('EventProgressUpdate', callback);
    }

    /** Listen for issue reports */
    onIssueReported(callback) {
        this.connection?.on('IssueReported', callback);
    }

    /** Listen for critical issue alerts (admins only) */
    onCriticalIssueAlert(callback) {
        this.connection?.on('CriticalIssueAlert', callback);
    }

    /** Listen for event completion */
    onEventCompleted(callback) {
        this.connection?.on('EventCompleted', callback);
    }

    /** Listen for payment status updates */
    onPaymentStatusUpdate(callback) {
        this.connection?.on('PaymentStatusUpdate', callback);
    }

    /** Remove a specific event listener */
    off(eventName) {
        this.connection?.off(eventName);
    }

    /** Disconnect from the hub */
    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }
}

export default new SupervisorTrackingService();
