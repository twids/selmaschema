import { Alert, Button, Card, CardContent, FormControl, InputLabel, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { changeRequestApi, familyApi, type ChangeRequestDto, type FamilyDto } from "../api/v2";
import type { ScheduleSide } from "../auth/AuthContext";

export function ChangeRequestsPage() {
  const { familyId = "" } = useParams();
  const [family, setFamily] = useState<FamilyDto | null>(null);
  const [items, setItems] = useState<ChangeRequestDto[]>([]);
  const [calendarId, setCalendarId] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [side, setSide] = useState<ScheduleSide>("A");
  const [message, setMessage] = useState("");
  const [error, setError] = useState<string | null>(null);
  const reload = useCallback(async () => {
    const [nextFamily, nextItems] = await Promise.all([familyApi.get(familyId), changeRequestApi.list(familyId)]);
    setFamily(nextFamily); setItems(nextItems); setCalendarId((current) => current || nextFamily.calendars[0]?.id || "");
  }, [familyId]);
  useEffect(() => { void reload().catch(() => setError("Bytesförfrågningarna kunde inte laddas.")); }, [reload]);

  const create = async () => {
    try { await changeRequestApi.create(familyId, calendarId, fromDate, toDate, side, message); setMessage(""); await reload(); }
    catch { setError("Förfrågan kunde inte skapas."); }
  };
  const review = async (id: string, approved: boolean) => {
    try { await changeRequestApi.review(familyId, id, approved); await reload(); }
    catch { setError("Förfrågan kunde inte granskas."); }
  };

  return <Stack spacing={3}><Typography variant="h4">Byten</Typography>{error && <Alert severity="error">{error}</Alert>}
    {family?.myPermission !== "Viewer" && <Card><CardContent><Stack spacing={2}><Typography variant="h6">Ny bytesförfrågan</Typography><FormControl><InputLabel>Kalender</InputLabel><Select label="Kalender" value={calendarId} onChange={(e) => setCalendarId(e.target.value)}>{family?.calendars.map((calendar) => <MenuItem key={calendar.id} value={calendar.id}>{calendar.name}</MenuItem>)}</Select></FormControl><Stack direction="row" spacing={2}><TextField type="date" label="Från" InputLabelProps={{ shrink: true }} value={fromDate} onChange={(e) => setFromDate(e.target.value)} /><TextField type="date" label="Till" InputLabelProps={{ shrink: true }} value={toDate} onChange={(e) => setToDate(e.target.value)} /><FormControl sx={{ minWidth: 140 }}><InputLabel>Önskad sida</InputLabel><Select label="Önskad sida" value={side} onChange={(e) => setSide(e.target.value as ScheduleSide)}><MenuItem value="A">{family?.sideALabel}</MenuItem><MenuItem value="B">{family?.sideBLabel}</MenuItem></Select></FormControl></Stack><TextField label="Meddelande" value={message} onChange={(e) => setMessage(e.target.value)} /><Button variant="contained" disabled={!calendarId || !fromDate || !toDate} onClick={() => void create()}>Skicka förfrågan</Button></Stack></CardContent></Card>}
    {items.map((item) => <Card key={item.id}><CardContent><Typography fontWeight={700}>{item.calendarName} · {item.fromDate}–{item.toDate}</Typography><Typography>{item.requestedByName} önskar {item.requestedSide} · {item.status}</Typography>{item.message && <Typography color="text.secondary">{item.message}</Typography>}{item.status === "Pending" && family?.myPermission !== "Viewer" && item.requestedByMemberId !== family?.myMemberId && <Stack direction="row" spacing={1} sx={{ mt: 1 }}><Button onClick={() => void review(item.id, true)}>Godkänn</Button><Button color="error" onClick={() => void review(item.id, false)}>Avslå</Button></Stack>}</CardContent></Card>)}
  </Stack>;
}
