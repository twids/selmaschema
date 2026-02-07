import { useState } from "react";
import { useAuth } from "../auth/AuthContext";

export default function AdminLogin({ onSuccess }: { onSuccess: () => void }) {
  const { loginAdmin } = useAuth();
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    const ok = await loginAdmin(password);
    setLoading(false);
    if (ok) onSuccess();
    else setError("Invalid admin password");
  };

  return (
    <div style={{ maxWidth: 420, margin: "2rem auto" }}>
      <h2>Admin Login</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            style={{ width: "100%", padding: "0.5rem", marginTop: 8 }}
          />
        </label>
        <button type="submit" disabled={loading} style={{ marginTop: 12 }}>
          {loading ? "Signing in…" : "Sign in"}
        </button>
      </form>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <p style={{ marginTop: 16 }}>
        Parents, use the magic link sent to you. It will automatically sign you
        in.
      </p>
    </div>
  );
}
