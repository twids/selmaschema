import {
  Alert, Box, Button, Card, CardContent, Checkbox, Dialog, DialogActions, DialogContent,
  DialogTitle, FormControl, FormControlLabel, InputLabel, MenuItem, Select, Stack, TextField, Typography,
} from "@mui/material";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import {
  familyApi, scheduleApi, type CalendarDayDto, type CalendarMonthDto, type FamilyDto,
  type ScheduleVersionDto,
} from "../api/v2";
import type { ScheduleSide } from "../auth/AuthContext";
import ScheduleEditor from "../components/ScheduleEditor";

export default function FamilyCalendarPage() {
  const { familyId = "", calendarId = "" } = useParams();
  const now = new Date();
  const [family, setFamily] = useState<FamilyDto | null>(null);
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [data, setData] = useState<CalendarMonthDto | null>(null);
  const [versions, setVersions] = useState<ScheduleVersionDto[]>([]);
  const [selected, setSelected] = useState<CalendarDayDto | null>(null);
  const [commentText, setCommentText] = useState("");
  const [error, setError] = useState<string | null>(null);
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

  const days = useMemo(() => data?.days ?? [], [data]);
  const navigateMonth = (delta: number) => {
    const next = new Date(year, month - 1 + delta, 1); setYear(next.getFullYear()); setMonth(next.getMonth() + 1);
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

    {isOwner && family && <ScheduleEditor family={family} calendarId={calendarId} versions={versions} onActivated={async () => { await load(); }} />}

    <Dialog open={selected !== null} onClose={() => setSelected(null)} fullWidth maxWidth="sm">
      <DialogTitle>{selected?.date}</DialogTitle>
      {selected && <DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
        <FormControl disabled={!canEdit}><InputLabel id="day-home-label">Hem</InputLabel><Select labelId="day-home-label" label="Hem" value={selected.side ?? ""} onChange={(e) => setSelected({ ...selected, side: (e.target.value || null) as ScheduleSide | null })}><MenuItem value="">Ej tilldelad</MenuItem><MenuItem value="A">{family?.sideALabel}</MenuItem><MenuItem value="B">{family?.sideBLabel}</MenuItem></Select></FormControl>
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
