import { useState } from "react";
import type { FormEvent } from "react";
import "./MiniGlassLogin.css";

interface MiniGlassLoginProps {
    onLoginSuccess: () => void;
    onLoginFailure: () => void;
}

export default function MiniGlassLogin({
    onLoginSuccess,
    onLoginFailure,
}: MiniGlassLoginProps) {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [loginFailed, setLoginFailed] = useState(false);
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        setLoginFailed(false);

        try {
            const response = await fetch("http://localhost:5000/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ username, password }),
            });

            if (response.ok) {
                const data = await response.json();
                localStorage.setItem("sessionToken", data.sessionToken);
                localStorage.setItem("tokenType", data.tokenType);
                onLoginSuccess();
            } else {
                setLoginFailed(true);
                onLoginFailure();
            }
        } catch (error) {
            console.error("Login error:", error);
            setLoginFailed(true);
            onLoginFailure();
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="mini-login">
            <h3 className="mini-login-title">Sign in</h3>

            <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Username"
                required
                autoFocus
            />

            <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Password"
                required
            />

            <button type="submit" disabled={loading}>
                {loading ? "Signing in…" : "Login"}
            </button>

            {loginFailed && (
                <span className="mini-login-error">
                    Invalid credentials
                </span>
            )}
        </form>
    );
}
