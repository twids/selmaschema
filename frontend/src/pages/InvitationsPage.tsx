import { useCallback, useEffect, useState } from "react";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import {
  Alert,
  Box,
  Button,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import { useAuth, type Role } from "../auth/AuthContext";
import {
  createInvitation,
  getInvitations,
  type CreatedInvitationDto,
  type InvitationDto,
} from "../api/invitations";

const statusText: Record<InvitationDto["status"], string> = {
  Pending: "Väntar",
  Consumed: "Använd",
  Expired: "Utgången",
};

export default function InvitationsPage() {
  const { user } = useAuth();
  const [role, setRole] = useState<Role>("ParentA");
  const [emailHint, setEmailHint] = useState("");
  const [invitations, setInvitations] = useState<InvitationDto[]>([]);
  const [created, setCreated] = useState<CreatedInvitationDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const load = useCallback(async () => {
    try {
      setInvitations(await getInvitations());
    } catch {
      setError("Kunde inte hämta inbjudningar.");
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const submit = async () => {
    setLoading(true);
    setError(null);
    setCreated(null);
    try {
      const result = await createInvitation(role, emailHint);
      setCreated(result);
      setEmailHint("");
      await load();
    } catch {
      setError("Kunde inte skapa inbjudningen. Kontrollera vald roll och försök igen.");
    } finally {
      setLoading(false);
    }
  };

  const copy = async () => {
    if (!created) return;
    await navigator.clipboard.writeText(created.invitationUrl);
    setCopied(true);
  };

  const formatDate = (value: string) =>
    new Date(value).toLocaleString("sv-SE", { dateStyle: "short", timeStyle: "short" });

  return (
    <Box data-testid="invitations-page">
      <Typography variant="h4" sx={{ mb: 3 }}>Inbjudningar</Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>Skapa inbjudan</Typography>
        <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap", alignItems: "center" }}>
          <TextField
            label="E-postledtråd (valfritt)"
            type="email"
            value={emailHint}
            onChange={(event) => setEmailHint(event.target.value)}
            sx={{ minWidth: 280 }}
          />
          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel id="invitation-role-label">Roll</InputLabel>
            <Select
              labelId="invitation-role-label"
              label="Roll"
              value={role}
              onChange={(event) => setRole(event.target.value as Role)}
            >
              <MenuItem value="ParentA">Förälder A</MenuItem>
              <MenuItem value="ParentB">Förälder B</MenuItem>
              {user?.role === "Admin" && <MenuItem value="Admin">Admin</MenuItem>}
            </Select>
          </FormControl>
          <Button variant="contained" onClick={submit} disabled={loading}>
            {loading ? "Skapar…" : "Skapa inbjudan"}
          </Button>
        </Box>
        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
        {created && (
          <Alert
            severity="success"
            sx={{ mt: 2, wordBreak: "break-all" }}
            action={
              <IconButton aria-label="Kopiera inbjudningslänk" onClick={copy}>
                <ContentCopyIcon />
              </IconButton>
            }
          >
            {copied ? "Länken är kopierad: " : "Kopiera länken nu – den visas bara här: "}
            {created.invitationUrl}
          </Alert>
        )}
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>Inbjudningar</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>E-postledtråd</TableCell>
              <TableCell>Roll</TableCell>
              <TableCell>Skapad av</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Utgår</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {invitations.length === 0 && (
              <TableRow><TableCell colSpan={5}>Inga inbjudningar ännu.</TableCell></TableRow>
            )}
            {invitations.map((invitation) => (
              <TableRow key={invitation.id}>
                <TableCell>{invitation.emailHint || "—"}</TableCell>
                <TableCell>{invitation.role}</TableCell>
                <TableCell>{invitation.createdByName}</TableCell>
                <TableCell>{statusText[invitation.status]}</TableCell>
                <TableCell>{formatDate(invitation.expiresAt)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Box>
  );
}
