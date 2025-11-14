// chat.js - SignalR Chat Client
(function () {
    'use strict';

    const API_BASE_URL = 'https://localhost:7160/api/chat';
    let connection = null;
    let isConnected = false;

    // Helper: Get JWT token
    function getAuthToken() {
        // Try meta tag
        const tokenMeta = document.querySelector('meta[name="jwt-token"]');
        if (tokenMeta) {
            return tokenMeta.getAttribute('content');
        }
        
        // Try hidden input
        const tokenInput = document.getElementById('jwtToken');
        if (tokenInput) {
            return tokenInput.value;
        }
        
        // Try cookie
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'JwtToken' || name === 'jwtToken') {
                return decodeURIComponent(value);
            }
        }
        
        return null;
    }

    // Initialize SignalR connection
    function initializeSignalR() {
        const token = getAuthToken();
        if (!token) {
            console.error('No JWT token found');
            return;
        }

        // Create connection with JWT token
        connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7160/chathub", {
                accessTokenFactory: () => token,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Handle connection events
        connection.onclose(() => {
            console.log('SignalR connection closed');
            isConnected = false;
        });

        connection.onreconnecting(() => {
            console.log('SignalR reconnecting...');
        });

        connection.onreconnected(() => {
            console.log('SignalR reconnected');
            isConnected = true;
        });

        // Listen for new messages
        connection.on("ReceiveMessage", (message) => {
            console.log('Received message:', message);
            displayMessage(message);
            scrollToBottom();
        });

        // Listen for notifications
        connection.on("NewMessageNotification", (notification) => {
            console.log('New message notification:', notification);
            showNotification(notification);
        });

        // Start connection
        connection.start()
            .then(() => {
                console.log('SignalR connected');
                isConnected = true;
            })
            .catch((err) => {
                console.error('SignalR connection error:', err);
            });
    }

    // Display message in chat
    function displayMessage(message) {
        const chatMessages = document.getElementById('chatMessages');
        if (!chatMessages) return;

        const currentUserId = parseInt(document.getElementById('currentUserId')?.value || '0');
        const isSender = message.senderId === currentUserId;

        // Check if message already exists (prevent duplicates)
        const existingMessage = chatMessages.querySelector(`[data-message-id="${message.id}"]`);
        if (existingMessage) {
            return;
        }

        const messageDiv = document.createElement('div');
        messageDiv.className = `chat-message ${isSender ? 'sent' : 'received'}`;
        messageDiv.setAttribute('data-message-id', message.id || Date.now());

        const bubbleDiv = document.createElement('div');
        bubbleDiv.className = `message-bubble ${isSender ? 'sent' : 'received'}`;

        const messageText = document.createElement('p');
        messageText.className = 'mb-1';
        messageText.textContent = message.message || '';

        const timeText = document.createElement('div');
        timeText.className = 'message-time';
        const sentDate = message.sentDate ? new Date(message.sentDate) : new Date();
        timeText.textContent = sentDate.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

        bubbleDiv.appendChild(messageText);
        bubbleDiv.appendChild(timeText);
        messageDiv.appendChild(bubbleDiv);
        chatMessages.appendChild(messageDiv);
    }

    // Scroll to bottom of chat
    function scrollToBottom() {
        const chatMessages = document.getElementById('chatMessages');
        if (chatMessages) {
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    }

    // Show notification
    function showNotification(notification) {
        console.log('Notification:', notification);
        
        // Create a simple toast notification
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification('New Message', {
                body: `${notification.senderName}: ${notification.preview}`,
                icon: '/assets/img/favicon.png'
            });
        }
        
        // Update page title if tab is not active
        if (document.hidden) {
            const originalTitle = document.title;
            document.title = `(1) ${originalTitle}`;
        }
    }

    // Request notification permission
    if ('Notification' in window && Notification.permission === 'default') {
        Notification.requestPermission();
    }

    // Send message
    async function sendMessage() {
        const messageInput = document.getElementById('messageInput');
        const sendButton = document.getElementById('sendButton');
        const customerIdInput = document.getElementById('customerId');
        const staffIdInput = document.getElementById('staffId');
        const currentUserIdInput = document.getElementById('currentUserId');

        if (!messageInput || !messageInput.value.trim()) {
            return;
        }

        const message = messageInput.value.trim();
        const customerId = parseInt(customerIdInput?.value || '0');
        const staffId = parseInt(staffIdInput?.value || '0');
        const currentUserId = parseInt(currentUserIdInput?.value || '0');

        if (customerId === 0 || staffId === 0 || currentUserId === 0) {
            alert('Invalid chat configuration. Please refresh the page.');
            return;
        }

        // Disable send button
        if (sendButton) {
            sendButton.disabled = true;
            sendButton.innerHTML = '<i class="bi bi-hourglass-split"></i> Sending...';
        }

        // ChatType sẽ được set tự động bởi backend dựa trên role
        // Validate trước khi gửi
        if (customerId === 0 || staffId === 0) {
            alert('Invalid chat configuration. Please refresh the page.');
            if (sendButton) {
                sendButton.disabled = false;
                sendButton.innerHTML = '<i class="bi bi-send"></i> Send';
            }
            return;
        }

        const payload = {
            customerId: customerId,
            staffId: staffId,
            message: message,
            senderId: currentUserId
        };

        try {
            const token = getAuthToken();
            if (!token) {
                throw new Error('No authentication token found');
            }

            const response = await fetch(`${API_BASE_URL}/send`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                messageInput.value = '';
                // Message will be displayed via SignalR
            } else {
                const errorData = await response.json().catch(() => ({ message: 'Unknown error' }));
                console.error('Error sending message:', errorData);
                
                // Hiển thị lỗi chi tiết hơn
                let errorMessage = errorData.message || 'Unknown error';
                if (errorData.errors && Array.isArray(errorData.errors)) {
                    const errorDetails = errorData.errors.map(e => `${e.field}: ${e.message}`).join('\n');
                    errorMessage += '\n' + errorDetails;
                }
                
                alert('Error sending message: ' + errorMessage);
            }
        } catch (error) {
            console.error('Error sending message:', error);
            alert('Error sending message. Please check your connection and try again.');
        } finally {
            // Re-enable send button
            if (sendButton) {
                sendButton.disabled = false;
                sendButton.innerHTML = '<i class="bi bi-send"></i> Send';
            }
        }
    }

    // Initialize chat for Customer
    window.initializeCustomerChat = function() {
        initializeSignalR();
        
        const sendButton = document.getElementById('sendButton');
        const messageInput = document.getElementById('messageInput');

        if (sendButton) {
            sendButton.addEventListener('click', sendMessage);
        }

        if (messageInput) {
            messageInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    sendMessage();
                }
            });
        }

        scrollToBottom();
    };

    // Initialize chat for Staff
    window.initializeStaffChat = function() {
        initializeSignalR();
        
        const sendButton = document.getElementById('sendButton');
        const messageInput = document.getElementById('messageInput');

        if (sendButton) {
            sendButton.addEventListener('click', sendMessage);
        }

        if (messageInput) {
            messageInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    sendMessage();
                }
            });
        }

        scrollToBottom();
    };

    // Auto-scroll on page load
    document.addEventListener('DOMContentLoaded', function() {
        scrollToBottom();
        
        // Restore page title when tab becomes visible
        document.addEventListener('visibilitychange', function() {
            if (!document.hidden) {
                document.title = document.title.replace(/^\(\d+\)\s*/, '');
            }
        });
    });

    // Export functions for global access
    window.chatFunctions = {
        sendMessage: sendMessage,
        scrollToBottom: scrollToBottom,
        displayMessage: displayMessage
    };
})();

