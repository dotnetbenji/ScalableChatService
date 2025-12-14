import { useState, useEffect } from "react";
import './Chat.css';
import GlassLogin from './GlassLogin';

interface ChatProps {
    onLoginRequiredAccepted: () => void;
}

export default function Chat({ onLoginRequiredAccepted }: ChatProps) {
    const [messages, setMessages] = useState<string[]>([]);
    const [newMessage, setNewMessage] = useState("");
    const [loginRequired, setLoginRequired] = useState(false);

    useEffect(() => {
        const eventSource = new EventSource('http://localhost:5001/sse');

        eventSource.onmessage = (event) => {
            setMessages(prev => [...prev, event.data]);
        };

        eventSource.onerror = (err) => {
            console.error('SSE error:', err);
            eventSource.close();
        };

        return () => {
            eventSource.close();
        };
    }, []);

    const handleSend = async () => {
        if (!newMessage.trim()) return;

        const token = localStorage.getItem('sessionToken');
        const tokenType = localStorage.getItem('tokenType');
        if (!token || !tokenType) {
            alert('No session token found. Please log in again.');
            return;
        }

        try {
            const response = await fetch('http://localhost:5001/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `${tokenType} ${token}`
                },
                body: JSON.stringify({ message: newMessage })
            });

            if (response.status === 401) {
                localStorage.removeItem('sessionToken');
                localStorage.removeItem('tokenType');
                setLoginRequired(true);
                return;
            }

            setNewMessage('');
        } catch (error) {
            console.error('Error sending message:', error);
        }
    };

    const handleLoginRequired = () => {
        onLoginRequiredAccepted();
        console.log('Login handler called');
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            handleSend();
        }
    };

    return (
        <div className="chat-container">
            <div className="chat-inner-container">
                <div className="messages">
                    {messages.map((msg, idx) => (
                        <div key={idx} className="chat-message">{msg}</div>
                    ))}
                </div>
                {loginRequired && (
                    <div className="login-required">
                        <div className="login-required-text">Login required</div>
                        <button
                            className="login-required-button"
                            onClick={handleLoginRequired}
                        >
                            Login
                        </button>
                    </div>
                )}
                <div className="chat-input-container">
                    <input
                        type="text"
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        onKeyDown={handleKeyDown}
                        placeholder="Type your message..."
                        className="chat-input"
                    />
                    <button onClick={handleSend} className="chat-send-button">Send</button>
                </div>
            </div>
        </div >
    );
}