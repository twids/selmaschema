import {
  Alert, Box, Button, Card, CardContent, Checkbox, Dialog, DialogActions, DialogContent,
  DialogTitle, FormControl, FormControlLabel, InputLabel, MenuItem, Select, Stack, TextField, Typography,
} from "@mui/material";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import {
  familyApi, scheduleApi, type CalendarDayDto, type CalendarMonthDto, type FamilyDto,
  type ScheduleDraft, type SchedulePreviewDto, type ScheduleTemplate, type ScheduleVersionDto,
} from "../api/v2";
import type { ScheduleSide } from "../auth/AuthContext";

const templateNames: Record<ScheduleTemplate, string> = {
  AlternatingWeeks: "Varannan vecka",
  TwoTwoThree: "2-2-3",
  TwoTwoFiveFive: "2-2-5-5",
  ThreeFourFourThree: "3-4-4-3",
  PrimaryAlternateWeekends: "Primärt boende + varannan helg",
};

const weekdays = [
  { value: 1, label: "Måndag" }, { value: 2, label: "Tisdag" }, { value: 3, label: "Onsdag" },
  { value: 4, label: "Torsdag" }, { value: 5, label: "Fredag" }, { value: 6, label: "Lördag" },
  { value: 0, label: "Söndag" },
];

function isoDate(date: Date) { return date.toISOString().slice(0, 10); }

export default function FamilyCalendarPage() {
  const { familyId = "", calendarId = "" } = useParams();
  const now = new Date();
  const tomorrow = new Date(now); tomorrow.setDate(tomorrow.getDate() + 1);
  const [family, setFamily] = useState<FamilyDto | null>(null);
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [data, setData] = useState<CalendarMonthDto | null>(null);
  const [versions, setVersions] = useState<ScheduleVersionDto[]>([]);
  const [preview, setPreview] = useState<SchedulePreviewDto | null>(null);
  const [selected, setSelected] = useState<CalendarDayDto | null>(null);
  const [commentText, setCommentText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [draft, setDraft] = useState<ScheduleDraft>({
    template: "AlternatingWeeks", anchorDate: isoDate(tomorrow), anchorSide: "A", effectiveFrom: isoDate(tomorrow),
    parameters: { weekendStartsOn: 5, weekendLengthDays: 3, recurringWeekday: null, recurringWeekdayOvernight: false },
    changeoverTime: null, changeoverPlace: null,
  });
  const calendar = family?.calendars.find((item) => item.id === calendarId);
  const canEdit = family?.myPermission !== "Viewer";
  const isOwner = family?.myPermission === "Owner";

  const load = useCallback(async () => {
    const [nextFamily, nextMonth, nextVersions] = await Promise.all([
      familyApi.get(familyId),
      scheduleApi.month(familyId, calendarId, year, month),
      scheduleApi.versions(familyId, calendarId),
    ]);
    setFamily(nextFamily); setData(nextMonth); setVersions(nextVersions);
  }, [familyId, calendarId, year, month]);
  useEffect(() => { void load().catch(() => setError("Kalendern kunde inte laddas.")); }, [load]);
  useEffect(() => { setPreview(null); }, [draft]);

  const days = useMemo(() => data?.days ?? [], [data]);
  const navigateMonth = (delta: number) => {
    const next = new Date(year, month - 1 + delta, 1); setYear(next.getFullYear()); setMonth(next.getMonth() + 1);
  };
  const previewDraft = async () => {
    setError(null);
    try { setPreview(await scheduleApi.preview(familyId, calendarId, draft)); }
    catch { setError("Schemat kunde inte förhandsvisas. Kontrollera startdatum och detaljnivå."); }
  };
  const activate = async () => {
    setError(null);
    try { await scheduleApi.create(familyId, calendarId, draft); setPreview(null); await load(); }
    catch { setError("Schemat kunde inte aktiveras. Förhandsvisa samma inställningar igen."); }
  };
  const saveDay = async () => {
    if (!selected || !canEdit) return;
    setError(null);
    try {
      await scheduleApi.setOverride(familyId, calendarId, selected.date, selected);
      setSelected(null);
      await load();
    } catch { setError("Dagen kunde inte sparas. Kontrollera tid och plats för familjens detaljnivå."); }
  };
  const clearDay = async () => {
    if (!selected?.isOverride || !canEdit) return;
    await scheduleApi.clearOverride(familyId, calendarId, selected.date);
    setSelected(null);
    await load();
  };
  const addComment = async () => {
    if (!selected || !commentText.trim() || !canEdit) return;
    try {
      const comment = await scheduleApi.addComment(familyId, calendarId, selected.date, commentText);
      setSelected({ ...selected, comments: [...selected.comments, comment] });
      setCommentText("");
      await load();
    } catch { setError("Kommentaren kunde inte sparas."); }
  };
  const deleteComment = async (commentId: string) => {
    if (!selected || !canEdit) return;
    try {
      await scheduleApi.deleteComment(familyId, commentId);
      setSelected({ ...selected, comments: selected.comments.filter((comment) => comment.id !== commentId) });
      await load();
    } catch { setError("Kommentaren kunde inte tas bort."); }
  };

  return <Stack spacing={3}>
    <Box><Typography variant="h4">{calendar?.name ?? "Kalender"}</Typography><Typography color="text.secondary">Öppna en dag för undantag, VAB, status och kommentarer.</Typography></Box>
    {error && <Alert severity="error">{error}</Alert>}
    <Stack direction="row" justifyContent="space-between" alignItems="center"><Button onClick={() => navigateMonth(-1)}>Föregående</Button><Typography variant="h6">{new Date(year, month - 1).toLocaleDateString("sv-SE", { month: "long", year: "numeric" })}</Typography><Button onClick={() => navigateMonth(1)}>Nästa</Button></Stack>
    <Box sx={{ display: "grid", gridTemplateColumns: "repeat(7, minmax(0, 1fr))", gap: 1 }}>
      {days.map((day) => <Card key={day.date} variant="outlined" sx={{ cursor: "pointer", bgcolor: day.side === "A" ? "primary.50" : day.side === "B" ? "secondary.50" : undefined, borderWidth: day.isOverride ? 2 : 1 }} onClick={() => setSelected({ ...day })}><CardContent sx={{ p: 1.5 }}><Typography fontWeight={700}>{new Date(`${day.date}T12:00:00`).getDate()}</Typography><Typography variant="body2">{day.side ? (day.side === "A" ? family?.sideALabel : family?.sideBLabel) : "Ej tilldelad"}</Typography><Typography variant="caption">{[day.isOverride && "Manuellt", day.isVab && "VAB", day.specialStatus, day.comments.length > 0 && `${day.comments.length} kommentar(er)`].filter(Boolean).join(" · ")}</Typography></CardContent></Card>)}
    </Box>

    {isOwner && <Card><CardContent><Stack spacing={2}>
      <Typography variant="h5">Nytt schema</Typography><Alert severity="info">Förhandsvisa först och välj ett framtida startdatum. Tidigare versioner och undantag bevaras.</Alert>
      <FormControl><InputLabel>Mall</InputLabel><Select label="Mall" value={draft.template} onChange={(e) => setDraft({ ...draft, template: e.target.value as ScheduleTemplate })}>{Object.entries(templateNames).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}</Select></FormControl>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
        <TextField type="date" label="Ankardatum" InputLabelProps={{ shrink: true }} value={draft.anchorDate} onChange={(e) => setDraft({ ...draft, anchorDate: e.target.value })} />
        <FormControl sx={{ minWidth: 160 }}><InputLabel>Startande sida</InputLabel><Select label="Startande sida" value={draft.anchorSide} onChange={(e) => setDraft({ ...draft, anchorSide: e.target.value as ScheduleSide })}><MenuItem value="A">{family?.sideALabel}</MenuItem><MenuItem value="B">{family?.sideBLabel}</MenuItem></Select></FormControl>
        <TextField type="date" label="Gäller från" InputLabelProps={{ shrink: true }} value={draft.effectiveFrom} onChange={(e) => setDraft({ ...draft, effectiveFrom: e.target.value })} />
      </Stack>
      {draft.template === "PrimaryAlternateWeekends" && <Stack spacing={2}><Stack direction={{ xs: "column", sm: "row" }} spacing={2}><FormControl sx={{ minWidth: 190 }}><InputLabel>Helgen börjar</InputLabel><Select label="Helgen börjar" value={draft.parameters.weekendStartsOn} onChange={(e) => setDraft({ ...draft, parameters: { ...draft.parameters, weekendStartsOn: Number(e.target.value) } })}>{weekdays.map((day) => <MenuItem key={day.value} value={day.value}>{day.label}</MenuItem>)}</Select></FormControl><TextField type="number" label="Helgens dagar" inputProps={{ min: 1, max: 7 }} value={draft.parameters.weekendLengthDays} onChange={(e) => setDraft({ ...draft, parameters: { ...draft.parameters, weekendLengthDays: Number(e.target.value) } })} /></Stack><FormControl><InputLabel>Återkommande vardag</InputLabel><Select label="Återkommande vardag" value={draft.parameters.recurringWeekday ?? ""} onChange={(e) => setDraft({ ...draft, parameters: { ...draft.parameters, recurringWeekday: String(e.target.value) === "" ? null : Number(e.target.value) } })}><MenuItem value="">Ingen</MenuItem>{weekdays.slice(0, 5).map((day) => <MenuItem key={day.value} value={day.value}>{day.label}</MenuItem>)}</Select></FormControl><FormControlLabel control={<Checkbox checked={draft.parameters.recurringWeekdayOvernight} disabled={draft.parameters.recurringWeekday == null} onChange={(e) => setDraft({ ...draft, parameters: { ...draft.parameters, recurringWeekdayOvernight: e.target.checked } })} />} label="Vardagsbesöket är en övernattning" /></Stack>}
      {family?.exchangeDetailLevel !== "Day" && <TextField type="time" label="Bytestid" InputLabelProps={{ shrink: true }} value={draft.changeoverTime ?? ""} onChange={(e) => setDraft({ ...draft, changeoverTime: e.target.value || null })} />}
      {family?.exchangeDetailLevel === "DayTimeAndPlace" && <TextField label="Bytesplats" value={draft.changeoverPlace ?? ""} onChange={(e) => setDraft({ ...draft, changeoverPlace: e.target.value || null })} />}
      <Stack direction="row" spacing={2}><Button variant="outlined" onClick={() => void previewDraft()}>Förhandsvisa 6 veckor</Button><Button variant="contained" disabled={!preview} onClick={() => void activate()}>Aktivera från valt datum</Button></Stack>
      {preview && <Box><Typography fontWeight={700}>Förhandsvisning giltig till {new Date(preview.validUntil).toLocaleTimeString("sv-SE")}</Typography><Typography>{preview.days.slice(0, 14).map((day) => day.side ?? "–").join(" · ")}</Typography></Box>}
      <Typography variant="h6">Historik</Typography>{versions.length === 0 ? <Typography color="text.secondary">Inget schema är aktiverat.</Typography> : versions.map((version) => <Typography key={version.id}>{templateNames[version.template]} · från {version.effectiveFrom}</Typography>)}
    </Stack></CardContent></Card>}

    <Dialog open={selected !== null} onClose={() => setSelected(null)} fullWidth maxWidth="sm">
      <DialogTitle>{selected?.date}</DialogTitle>
      {selected && <DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
        <FormControl disabled={!canEdit}><InputLabel>Hem</InputLabel><Select label="Hem" value={selected.side ?? ""} onChange={(e) => setSelected({ ...selected, side: (e.target.value || null) as ScheduleSide | null })}><MenuItem value="">Ej tilldelad</MenuItem><MenuItem value="A">{family?.sideALabel}</MenuItem><MenuItem value="B">{family?.sideBLabel}</MenuItem></Select></FormControl>
        <FormControlLabel control={<Checkbox disabled={!canEdit} checked={selected.isVab} onChange={(e) => setSelected({ ...selected, isVab: e.target.checked })} />} label="VAB" />
        <TextField disabled={!canEdit} label="Specialstatus" value={selected.specialStatus ?? ""} onChange={(e) => setSelected({ ...selected, specialStatus: e.target.value || null })} />
        {family?.exchangeDetailLevel !== "Day" && <TextField disabled={!canEdit} type="time" label="Bytestid" InputLabelProps={{ shrink: true }} value={selected.changeoverTime ?? ""} onChange={(e) => setSelected({ ...selected, changeoverTime: e.target.value || null })} />}
        {family?.exchangeDetailLevel === "DayTimeAndPlace" && <TextField disabled={!canEdit} label="Bytesplats" value={selected.changeoverPlace ?? ""} onChange={(e) => setSelected({ ...selected, changeoverPlace: e.target.value || null })} />}
        <Typography variant="h6">Kommentarer</Typography>
        {selected.comments.length === 0 && <Typography color="text.secondary">Inga kommentarer.</Typography>}
        {selected.comments.map((comment) => <Box key={comment.id}><Typography><strong>{comment.authorName}</strong> · {new Date(comment.createdAt).toLocaleString("sv-SE")}</Typography><Typography>{comment.text}</Typography>{canEdit && (family?.myPermission === "Owner" || comment.authorMemberId === family?.myMemberId) && <Button size="small" color="error" onClick={() => void deleteComment(comment.id)}>Ta bort</Button>}</Box>)}
        {canEdit && <Stack direction="row" spacing={1}><TextField fullWidth label="Ny kommentar" value={commentText} onChange={(e) => setCommentText(e.target.value)} /><Button disabled={!commentText.trim()} onClick={() => void addComment()}>Lägg till</Button></Stack>}
      </Stack></DialogContent>}
      <DialogActions>{selected?.isOverride && canEdit && <Button color="error" onClick={() => void clearDay()}>Återgå till grundschema</Button>}<Button onClick={() => setSelected(null)}>Stäng</Button>{canEdit && <Button variant="contained" onClick={() => void saveDay()}>Spara dag</Button>}</DialogActions>
    </Dialog>
  </Stack>;
}
