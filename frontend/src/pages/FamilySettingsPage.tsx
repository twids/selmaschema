import { Alert, Box, Button, Card, CardContent, FormControl, InputLabel, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { familyApi, type ChildDto, type FamilyDto, type MemberDto, type ResidenceCalendarDto } from "../api/v2";

export default function FamilySettingsPage() {
  const { familyId = "" } = useParams();
  const [family, setFamily] = useState<FamilyDto | null>(null);
  const [children, setChildren] = useState<ChildDto[]>([]);
  const [calendars, setCalendars] = useState<ResidenceCalendarDto[]>([]);
  const [members, setMembers] = useState<MemberDto[]>([]);
  const [childName, setChildName] = useState("");
  const [childCalendarId, setChildCalendarId] = useState("");
  const [calendarName, setCalendarName] = useState("");
  const [memberReason, setMemberReason] = useState("");
  const [message, setMessage] = useState<string | null>(null);

  const reload = useCallback(async () => {
    const [nextFamily, nextChildren, nextCalendars, nextMembers] = await Promise.all([
      familyApi.get(familyId), familyApi.children(familyId), familyApi.calendars(familyId), familyApi.members(familyId),
    ]);
    setFamily(nextFamily); setChildren(nextChildren); setCalendars(nextCalendars); setMembers(nextMembers);
    setChildCalendarId((current) => current || nextCalendars[0]?.id || "");
  }, [familyId]);
  useEffect(() => { void reload(); }, [reload]);

  if (!family) return null;
  if (family.myPermission !== "Owner") return <Alert severity="warning">Bara familjens ägare kan ändra inställningarna.</Alert>;

  return (
    <Stack spacing={3}>
      <Typography variant="h4">Familjeinställningar</Typography>
      {message && <Alert severity="success">{message}</Alert>}
      <Card><CardContent><Typography variant="h6" gutterBottom>Grunduppgifter</Typography>
        <Stack spacing={2}>
          <TextField label="Familjens namn" value={family.name} onChange={(e) => setFamily({ ...family, name: e.target.value })} />
          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <TextField label="Sida A" value={family.sideALabel} onChange={(e) => setFamily({ ...family, sideALabel: e.target.value })} />
            <TextField label="Sida B" value={family.sideBLabel} onChange={(e) => setFamily({ ...family, sideBLabel: e.target.value })} />
            <TextField label="Tidszon" value={family.timeZoneId} onChange={(e) => setFamily({ ...family, timeZoneId: e.target.value })} />
          </Stack>
          <FormControl><InputLabel id="exchange-detail-level-label">Detaljnivå vid byte</InputLabel><Select labelId="exchange-detail-level-label" label="Detaljnivå vid byte" value={family.exchangeDetailLevel} onChange={(e) => setFamily({ ...family, exchangeDetailLevel: e.target.value as FamilyDto["exchangeDetailLevel"] })}>
            <MenuItem value="Day">Dag</MenuItem><MenuItem value="DayAndTime">Dag + tid</MenuItem><MenuItem value="DayTimeAndPlace">Dag + tid + plats</MenuItem>
          </Select></FormControl>
          <Button variant="contained" onClick={() => void familyApi.update(familyId, {
            name: family.name, timeZoneId: family.timeZoneId, sideALabel: family.sideALabel,
            sideBLabel: family.sideBLabel, exchangeDetailLevel: family.exchangeDetailLevel,
          }).then((value) => { setFamily(value); setMessage("Inställningarna är sparade."); })}>Spara inställningar</Button>
        </Stack>
      </CardContent></Card>
      <Card><CardContent><Typography variant="h6" gutterBottom>Barn</Typography>
        {children.filter((child) => child.isActive).map((child) => <Typography key={child.id}>{child.displayName} · {calendars.find((calendar) => calendar.id === child.calendarId)?.name ?? "ingen kalender"}</Typography>)}
        <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ mt: 2 }}>
          <TextField label="Visningsnamn" value={childName} onChange={(e) => setChildName(e.target.value)} />
          <FormControl sx={{ minWidth: 200 }}><InputLabel id="child-calendar-label">Kalender</InputLabel><Select labelId="child-calendar-label" label="Kalender" value={childCalendarId} onChange={(e) => setChildCalendarId(e.target.value)}>{calendars.map((calendar) => <MenuItem key={calendar.id} value={calendar.id}>{calendar.name}</MenuItem>)}</Select></FormControl>
          <Button disabled={!childName.trim() || !childCalendarId} onClick={() => void familyApi.createChild(familyId, childName, childCalendarId).then(() => { setChildName(""); return reload(); })}>Lägg till barn</Button>
        </Stack>
      </CardContent></Card>
      <Card><CardContent><Typography variant="h6" gutterBottom>Kalendrar</Typography>
        {calendars.map((calendar) => <Typography key={calendar.id}>{calendar.name} · {calendar.children.length} barn</Typography>)}
        <Stack direction="row" spacing={2} sx={{ mt: 2 }}><TextField label="Namn" value={calendarName} onChange={(e) => setCalendarName(e.target.value)} /><Button disabled={!calendarName.trim()} onClick={() => void familyApi.createCalendar(familyId, calendarName).then(() => { setCalendarName(""); return reload(); })}>Ny kalender</Button></Stack>
      </CardContent></Card>
      <Card><CardContent><Typography variant="h6" gutterBottom>Medlemmar</Typography>
        <TextField fullWidth label="Orsak till medlemsändring" value={memberReason} onChange={(e) => setMemberReason(e.target.value)} sx={{ mb: 2 }} />
        <Stack spacing={2}>{members.filter((member) => member.isActive).map((member) => (
          <Box key={member.id}><Typography>{member.displayName} · {member.permission} · {member.side === "A" ? family.sideALabel : member.side === "B" ? family.sideBLabel : "inget hem"}</Typography>
            <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ mt: 1 }}>
              {member.permission !== "Owner" && <><Button size="small" disabled={!memberReason.trim()} onClick={() => void familyApi.updateMember(familyId, member.id, "Editor", member.side, memberReason).then(reload)}>Editor</Button><Button size="small" disabled={!memberReason.trim()} onClick={() => void familyApi.updateMember(familyId, member.id, "Viewer", member.side, memberReason).then(reload)}>Viewer</Button></>}
              <Button size="small" disabled={!memberReason.trim()} onClick={() => void familyApi.updateMember(familyId, member.id, member.permission, "A", memberReason).then(reload)}>{family.sideALabel}</Button>
              <Button size="small" disabled={!memberReason.trim()} onClick={() => void familyApi.updateMember(familyId, member.id, member.permission, "B", memberReason).then(reload)}>{family.sideBLabel}</Button>
              <Button size="small" disabled={!memberReason.trim()} onClick={() => void familyApi.updateMember(familyId, member.id, member.permission, null, memberReason).then(reload)}>Ingen sida</Button>
              {member.permission !== "Owner" && <Button size="small" color="warning" disabled={!memberReason.trim()} onClick={() => void familyApi.transferOwnership(familyId, member.id, memberReason).then(reload)}>Överför ägarskap</Button>}
            </Stack>
          </Box>
        ))}</Stack>
      </CardContent></Card>
    </Stack>
  );
}
