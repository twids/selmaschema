import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { adminApi, type AdminFamilyDetailDto, type AdminFamilyListItemDto, type FamilyStatus } from "../api/v2";

const templateNames = {
  AlternatingWeeks: "Varannan vecka",
  TwoTwoThree: "2-2-3",
  TwoTwoFiveFive: "2-2-5-5",
  ThreeFourFourThree: "3-4-4-3",
  PrimaryAlternateWeekends: "Primärt boende + varannan helg",
};

export default function PlatformAdminPage() {
  const [families, setFamilies] = useState<AdminFamilyListItemDto[]>([]);
  const [selected, setSelected] = useState<AdminFamilyDetailDto | null>(null);
  const [reason, setReason] = useState("");
  const [supportName, setSupportName] = useState("");
  const [supportTimeZone, setSupportTimeZone] = useState("");
  const [error, setError] = useState<string | null>(null);
  const load = () => adminApi.families().then(setFamilies).catch(() => setError("Administrationsdata kunde inte laddas."));
  useEffect(() => { void load(); }, []);
  const open = async (id: string) => {
    const value = await adminApi.family(id); setSelected(value); setSupportName(value.family.name); setSupportTimeZone(value.family.timeZoneId);
  };
  const updateStatus = async (status: FamilyStatus) => {
    if (!selected || !reason.trim()) return;
    await adminApi.updateFamily(selected.family.id, { status, reason });
    setSelected(await adminApi.family(selected.family.id)); setReason(""); await load();
  };
  const revokeSessions = async (accountId: string) => {
    if (!selected || !reason.trim()) return;
    await adminApi.revokeSessions(accountId, reason); setReason(""); setSelected(await adminApi.family(selected.family.id));
  };
  const correctFamily = async () => {
    if (!selected || !reason.trim()) return;
    await adminApi.updateFamily(selected.family.id, { name: supportName, timeZoneId: supportTimeZone, reason });
    setSelected(await adminApi.family(selected.family.id)); setReason(""); await load();
  };
  const correctMember = async (memberId: string, permission: "Owner" | "Editor" | "Viewer", side: "A" | "B" | null) => {
    if (!selected || !reason.trim()) return;
    await adminApi.updateMember(selected.family.id, memberId, permission, side, reason);
    setSelected(await adminApi.family(selected.family.id)); setReason("");
  };
  const transfer = async (memberId: string) => {
    if (!selected || !reason.trim()) return;
    await adminApi.transferOwnership(selected.family.id, memberId, reason);
    setSelected(await adminApi.family(selected.family.id)); setReason("");
  };
  const revokeInvitation = async (invitationId: string) => {
    if (!selected || !reason.trim()) return;
    await adminApi.revokeInvitation(selected.family.id, invitationId, reason);
    setSelected(await adminApi.family(selected.family.id)); setReason("");
  };

  return <Stack spacing={3}><Box><Typography variant="h3" fontWeight={700}>Familjer</Typography><Typography color="text.secondary">Supportåtgärder kräver orsak och auditeras. Privat kalendertext visas aldrig här.</Typography></Box>{error && <Alert severity="error">{error}</Alert>}
    <Box sx={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 2 }}>{families.map((family) => <Card key={family.id}><CardActionArea onClick={() => void open(family.id)}><CardContent><Stack direction="row" justifyContent="space-between"><Typography variant="h6">{family.name}</Typography><Chip label={family.status} color={family.status === "Active" ? "success" : "warning"} size="small" /></Stack><Typography>{family.memberCount} medlemmar · {family.calendarCount} kalendrar</Typography><Typography color="text.secondary">{family.scheduleVersionCount} schemaversioner</Typography></CardContent></CardActionArea></Card>)}</Box>
    <Dialog open={selected !== null} onClose={() => setSelected(null)} fullWidth maxWidth="md"><DialogTitle>{selected?.family.name}</DialogTitle><DialogContent><Stack spacing={3} sx={{ mt: 1 }}>
      <Stack direction="row" spacing={2}><FormControl sx={{ minWidth: 200 }}><InputLabel>Status</InputLabel><Select label="Status" value={selected?.family.status ?? "Active"} disabled><MenuItem value="Active">Aktiv</MenuItem><MenuItem value="Suspended">Avstängd</MenuItem></Select></FormControl><TextField fullWidth label="Obligatorisk orsak" value={reason} onChange={(e) => setReason(e.target.value)} /></Stack>
      <Stack direction="row" spacing={2}><TextField label="Familjenamn" value={supportName} onChange={(e) => setSupportName(e.target.value)} /><TextField label="Tidszon" value={supportTimeZone} onChange={(e) => setSupportTimeZone(e.target.value)} /><Button disabled={!reason.trim()} onClick={() => void correctFamily()}>Spara korrigering</Button></Stack>
      <Stack direction="row" spacing={2}><Button disabled={!reason.trim()} color="warning" onClick={() => void updateStatus("Suspended")}>Stäng av familj</Button><Button disabled={!reason.trim()} onClick={() => void updateStatus("Active")}>Återaktivera</Button></Stack>
      <Typography variant="h6">Medlemmar</Typography>{selected?.members.map((member) => <Stack key={member.id} spacing={1}><Typography>{member.displayName} · {member.email} · {member.permission} · {member.side ?? "–"}</Typography><Stack direction="row" spacing={1} flexWrap="wrap">{member.permission !== "Owner" && <><Button size="small" disabled={!reason.trim()} onClick={() => void correctMember(member.id, "Editor", member.side)}>Editor</Button><Button size="small" disabled={!reason.trim()} onClick={() => void correctMember(member.id, "Viewer", member.side)}>Viewer</Button></>}<Button size="small" disabled={!reason.trim()} onClick={() => void correctMember(member.id, member.permission, "A")}>Hem A</Button><Button size="small" disabled={!reason.trim()} onClick={() => void correctMember(member.id, member.permission, "B")}>Hem B</Button><Button size="small" disabled={!reason.trim()} onClick={() => void correctMember(member.id, member.permission, null)}>Ingen sida</Button>{member.permission !== "Owner" && <Button size="small" color="warning" disabled={!reason.trim()} onClick={() => void transfer(member.id)}>Överför ägarskap</Button>}<Button size="small" disabled={!reason.trim()} onClick={() => void revokeSessions(member.accountId)}>Återkalla sessioner</Button></Stack></Stack>)}
      <Typography variant="h6">Kalendrar</Typography>{selected?.calendars.map((calendar) => <Typography key={calendar.id}>{calendar.name} · {calendar.childCount} barn · {calendar.isActive ? "aktiv" : "arkiverad"}</Typography>)}
      <Typography variant="h6">Schemaversioner</Typography>{selected?.scheduleVersions.length === 0 ? <Typography color="text.secondary">Inga schemaversioner.</Typography> : selected?.scheduleVersions.map((version) => <Typography key={version.id}>{version.calendarName} · {templateNames[version.template]} · från {version.effectiveFrom}</Typography>)}
      <Typography variant="h6">Inbjudningar</Typography>{selected?.invitations.length === 0 ? <Typography color="text.secondary">Inga inbjudningar.</Typography> : selected?.invitations.map((invitation) => <Stack key={invitation.id} direction="row" spacing={1} alignItems="center"><Typography>{invitation.emailHint ?? "Ingen e-postledtråd"} · {invitation.permission} · {invitation.status}</Typography>{invitation.status === "Active" && <Button size="small" color="error" disabled={!reason.trim()} onClick={() => void revokeInvitation(invitation.id)}>Återkalla</Button>}</Stack>)}
      <Typography variant="h6">Senaste aktivitet</Typography>{selected?.activity.map((event) => <Typography key={event.id} variant="body2">{new Date(event.createdAt).toLocaleString("sv-SE")} · {event.action} · {event.reason}</Typography>)}
    </Stack></DialogContent><DialogActions><Button onClick={() => setSelected(null)}>Stäng</Button></DialogActions></Dialog>
  </Stack>;
}
