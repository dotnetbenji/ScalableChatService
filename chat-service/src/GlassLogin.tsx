import { useState } from "react";
import './GlassLogin.css';

interface GlassLoginProps {
  onLoginSuccess: () => void;
}

export default function GlassLogin({ onLoginSuccess }: GlassLoginProps) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loginFailed, setLoginFailed] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const response = await fetch('http://localhost:5000/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username, password }),
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('sessionToken', data.sessionToken);
        localStorage.setItem('tokenType', data.tokenType);
        setLoginFailed(false);
        onLoginSuccess();
      } else if (response.status === 401) {
        setLoginFailed(true);
      } else {
        setLoginFailed(true);
      }
    } catch (error) {
      console.error('Network or server error:', error);
      setLoginFailed(true);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1 className="login-title">Sign In</h1>
        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label>Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter your username"
              required
              className="form-input"
            />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
              className="form-input"
            />
          </div>
          <button type="submit" className="form-button">Login</button>
          {loginFailed && (
            <div className="login-error">Login failed.</div>
          )}
        </form>
      </div>
    </div>
  );
}