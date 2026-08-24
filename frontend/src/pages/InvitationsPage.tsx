import { Alert, Button, Card, CardContent, FormControl, InputLabel, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { familyApi, type CreatedInvitationDto, type InvitationDto } from "../api/v2";
import type { ScheduleSide } from "../auth/AuthContext";

export default function InvitationsPage() {
  const { familyId = "" } = useParams();
  const [items, setItems] = useState<InvitationDto[]>([]);
  const [permission, setPermission] = useState<"Editor" | "Viewer">("Editor");
  const [side, setSide] = useState<ScheduleSide | "">("");
  const [emailHint, setEmailHint] = useState("");
  const [created, setCreated] = useState<CreatedInvitationDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const reload = useCallback(() => familyApi.invitations(familyId).then(setItems).catch(() => setError("Bara familjens ägare kan hantera inbjudningar.")), [familyId]);
  useEffect(() => { void reload(); }, [reload]);

  const create = async () => {
    setError(null);
    try { const value = await familyApi.createInvitation(familyId, permission, side || null, emailHint); setCreated(value); await reload(); }
    catch { setError("Inbjudan kunde inte skapas."); }
  };
  const copy = async (value: string) => { await navigator.clipboard.writeText(value); };

  return <Stack spacing={3}>
    <Typography variant="h4">Bjud in till familjen</Typography>
    {error && <Alert severity="error">{error}</Alert>}
    <Card><CardContent><Stack spacing={2}>
      <Typography>Varje inbjudan får både en magisk länk och en kort engångskod. Den gäller i sju dagar.</Typography>
      <FormControl><InputLabel id="invitation-permission-label">Behörighet</InputLabel><Select labelId="invitation-permission-label" label="Behörighet" value={permission} onChange={(e) => setPermission(e.target.value as "Editor" | "Viewer")}><MenuItem value="Editor">Kan redigera</MenuItem><MenuItem value="Viewer">Kan läsa</MenuItem></Select></FormControl>
      <FormControl><InputLabel id="invitation-side-label">Schemasida</InputLabel><Select labelId="invitation-side-label" label="Schemasida" value={side} onChange={(e) => setSide(e.target.value as ScheduleSide | "")}><MenuItem value="">Ingen</MenuItem><MenuItem value="A">Hem A</MenuItem><MenuItem value="B">Hem B</MenuItem></Select></FormControl>
      <TextField label="E-postledtråd (valfri)" value={emailHint} onChange={(e) => setEmailHint(e.target.value)} />
      <Button variant="contained" onClick={() => void create()}>Skapa inbjudan</Button>
    </Stack></CardContent></Card>
    {created && <Alert severity="success"><Stack spacing={1}><Typography fontWeight={700}>Visa detta nu – värdena kan inte hämtas igen.</Typography><Typography sx={{ wordBreak: "break-all" }}>{created.link}</Typography><Button onClick={() => void copy(created.link)}>Kopiera länk</Button><Typography variant="h5">{created.code}</Typography><Button onClick={() => void copy(created.code)}>Kopiera kod</Button></Stack></Alert>}
    <Card><CardContent><Typography variant="h6" gutterBottom>Tidigare inbjudningar</Typography>{items.map((item) => <Typography key={item.id}>{item.emailHint || "Ingen e-postledtråd"} · {item.permission} · {item.status} · går ut {new Date(item.expiresAt).toLocaleString("sv-SE")}</Typography>)}</CardContent></Card>
  </Stack>;
}
