import { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";

const API_BASE_URL = import.meta.env.VITE_API_URL || "";

interface MagicLinkItem {
  token: string;
  magicLink: string;
  email: string;
  role: string;
  displayName: string | null;
  expiresAt: string;
}

interface UserItem {
  id: number;
  email: string;
  role: string;
  displayName: string | null;
  lastLoginAt: string | null;
}

export default function AdminDashboard() {
  const { authHeader, user, logout } = useAuth();
  const [email, setEmail] = useState("");
  const [role, setRole] = useState("ParentA");
  const [displayName, setDisplayName] = useState("");
  const [creating, setCreating] = useState(false);
  const [magicLinks, setMagicLinks] = useState<MagicLinkItem[]>([]);
  const [users, setUsers] = useState<UserItem[]>([]);

  const isAdmin = user?.role === "Admin";

  const refreshData = async () => {
    try {
      const [linksRes, usersRes] = await Promise.all([
        fetch(`${API_BASE_URL}/api/admin/magic-links`, {
          headers: authHeader(),
        }),
        fetch(`${API_BASE_URL}/api/admin/users`, {
          headers: authHeader(),
        }),
      ]);
      if (linksRes.ok) {
        const data = await linksRes.json();
        setMagicLinks(data);
      }
      if (usersRes.ok) {
        const data = await usersRes.json();
        setUsers(data);
      }
    } catch (err) {
      console.error("Failed to load admin data", err);
    }
  };

  useEffect(() => {
    if (isAdmin) refreshData();
  }, [isAdmin]);

  const createMagicLink = async () => {
    setCreating(true);
    try {
      const res = await fetch(`${API_BASE_URL}/api/admin/magic-links`, {
        method: "POST",
        headers: { "Content-Type": "application/json", ...authHeader() },
        body: JSON.stringify({ email, role, displayName }),
      });
      setCreating(false);
      if (!res.ok) return;
      await refreshData();
      setEmail("");
      setDisplayName("");
    } catch (err) {
      setCreating(false);
    }
  };

  if (!isAdmin) {
    return (
      <div style={{ maxWidth: 720, margin: "2rem auto" }}>
        <h2>Admin Only</h2>
        <p>You must be signed in as an administrator.</p>
        <button onClick={logout}>Log out</button>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 960, margin: "2rem auto" }}>
      <h2>Admin Dashboard</h2>
      <section style={{ marginTop: 16 }}>
        <h3>Create Magic Link</h3>
        <div
          style={{
            display: "grid",
            gap: 8,
            gridTemplateColumns: "1fr 160px 1fr auto",
          }}
        >
          <input
            placeholder="email@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <select value={role} onChange={(e) => setRole(e.target.value)}>
            <option value="ParentA">Parent A</option>
            <option value="ParentB">Parent B</option>
          </select>
          <input
            placeholder="Display name (optional)"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
          />
          <button disabled={creating} onClick={createMagicLink}>
            {creating ? "Creating…" : "Create"}
          </button>
        </div>
      </section>
      <section style={{ marginTop: 24 }}>
        <h3>Pending Magic Links</h3>
        <ul>
          {magicLinks.map((m) => (
            <li key={m.token}>
              {m.email} → {m.role} — expires{" "}
              {new Date(m.expiresAt).toLocaleString()} —
              <a href={m.magicLink} style={{ marginLeft: 8 }}>
                Open Link
              </a>
            </li>
          ))}
          {magicLinks.length === 0 && <li>No pending magic links</li>}
        </ul>
      </section>
      <section style={{ marginTop: 24 }}>
        <h3>Users</h3>
        <ul>
          {users.map((u) => (
            <li key={u.id}>
              {u.displayName || u.email} — {u.role} — last login{" "}
              {u.lastLoginAt
                ? new Date(u.lastLoginAt).toLocaleString()
                : "never"}
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
