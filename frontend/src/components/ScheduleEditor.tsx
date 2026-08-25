import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import {
  Accordion, AccordionDetails, AccordionSummary, Alert, Box, Button, Card, CardActionArea, CardContent,
  Checkbox, CircularProgress, FormControl, FormControlLabel, FormHelperText, InputLabel, MenuItem, Select, Stack, TextField, Typography,
} from "@mui/material";
import { useState } from "react";
import { scheduleApi, type FamilyDto, type ScheduleDraft, type SchedulePreviewDto, type ScheduleTemplate, type ScheduleVersionDto } from "../api/v2";
import type { ScheduleSide } from "../auth/AuthContext";
import SchedulePreview from "./SchedulePreview";
import { homeName, localIsoDate, patternForAnchor, scheduleTemplates, weekdayOptions } from "./schedulePresentation";

interface ScheduleEditorProps {
  family: FamilyDto;
  calendarId: string;
  versions?: ScheduleVersionDto[];
  showHistory?: boolean;
  onActivated?: (version: ScheduleVersionDto) => void | Promise<void>;
}

function defaultDraft(side: ScheduleSide): ScheduleDraft {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const start = localIsoDate(tomorrow);
  return {
    template: "AlternatingWeeks",
    anchorDate: start,
    anchorSide: side,
    effectiveFrom: start,
    parameters: { weekendStartsOn: 5, weekendLengthDays: 3, recurringWeekday: null, recurringWeekdayOvernight: false },
    changeoverTime: null,
    changeoverPlace: null,
  };
}

export default function ScheduleEditor({ family, calendarId, versions = [], showHistory = true, onActivated }: ScheduleEditorProps) {
  const [draft, setDraft] = useState<ScheduleDraft>(() => defaultDraft(family.mySide ?? "A"));
  const [preview, setPreview] = useState<SchedulePreviewDto | null>(null);
  const [advanced, setAdvanced] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const startHomeLabel = advanced ? "Hem på ankardatumet" : "Barnet börjar hos";

  const updateDraft = (next: ScheduleDraft) => {
    setDraft(next);
    setPreview(null);
    setError(null);
  };

  const updateStartDate = (effectiveFrom: string) => {
    updateDraft({ ...draft, effectiveFrom, anchorDate: advanced ? draft.anchorDate : effectiveFrom });
  };

  const previewDraft = async () => {
    setBusy(true);
    setError(null);
    try {
      setPreview(await scheduleApi.preview(family.id, calendarId, draft));
    } catch {
      setError("Schemat kunde inte förhandsvisas. Kontrollera startdatumet och de valda bytesdetaljerna.");
    } finally {
      setBusy(false);
    }
  };

  const activate = async () => {
    if (!preview) return;
    if (new Date(preview.validUntil).getTime() <= Date.now()) {
      setPreview(null);
      setError("Förhandsvisningen har gått ut. Förhandsvisa samma inställningar igen innan du aktiverar schemat.");
      return;
    }
    setBusy(true);
    setError(null);
    let version: ScheduleVersionDto;
    try {
      version = await scheduleApi.create(family.id, calendarId, draft);
    } catch {
      setError("Schemat kunde inte aktiveras. Förhandsvisa samma inställningar igen och kontrollera att startdatumet ligger i framtiden.");
      setBusy(false);
      return;
    }
    setPreview(null);
    try {
      await onActivated?.(version);
    } catch {
      setError("Schemat aktiverades, men sidan kunde inte uppdateras. Ladda om sidan för att se den nya versionen.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <Card>
      <CardContent>
        <Stack spacing={3}>
          <Box>
            <Typography variant="h5">Nytt schema</Typography>
            <Typography color="text.secondary">Välj ett välkänt boendemönster. Du ser exakt hur dagarna fördelas innan något aktiveras.</Typography>
          </Box>
          {error && <Alert severity="error">{error}</Alert>}

          <Box>
            <Typography fontWeight={700} gutterBottom>1. Välj boendemönster</Typography>
            <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))" }, gap: 1.5 }}>
              {(Object.entries(scheduleTemplates) as [ScheduleTemplate, (typeof scheduleTemplates)[ScheduleTemplate]][]).map(([template, metadata]) => {
                const selected = draft.template === template;
                const pattern = patternForAnchor(metadata.pattern, draft.anchorSide);
                const firstHome = draft.anchorSide === "A" ? family.sideALabel : family.sideBLabel;
                const secondHome = draft.anchorSide === "A" ? family.sideBLabel : family.sideALabel;
                return (
                  <Card key={template} variant="outlined" sx={{ borderWidth: selected ? 2 : 1, borderColor: selected ? "primary.main" : "divider" }}>
                    <CardActionArea
                      aria-pressed={selected}
                      onClick={() => updateDraft({ ...draft, template })}
                      sx={{ height: "100%" }}
                    >
                      <CardContent>
                        <Typography fontWeight={700}>{metadata.name}</Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ minHeight: { md: 44 }, mb: 1.5 }}>
                          {metadata.description(firstHome, secondHome)}
                        </Typography>
                        <Stack direction="row" spacing={0.35} aria-label={`Tvåveckorsmönster för ${metadata.name}`}>
                          {pattern.map((side, index) => (
                            <Box
                              key={`${side}-${index}`}
                              title={homeName(side, family.sideALabel, family.sideBLabel)}
                              sx={{ height: 14, flex: 1, borderRadius: 0.5, bgcolor: side === "A" ? "primary.main" : "secondary.main" }}
                            />
                          ))}
                        </Stack>
                      </CardContent>
                    </CardActionArea>
                  </Card>
                );
              })}
            </Box>
          </Box>

          <Box>
            <Typography fontWeight={700} gutterBottom>2. Välj start</Typography>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
              <TextField
                type="date"
                label="Schemat börjar"
                helperText="Den första dag då det nya schemat ska gälla."
                slotProps={{ inputLabel: { shrink: true } }}
                value={draft.effectiveFrom}
                onChange={(event) => updateStartDate(event.target.value)}
                fullWidth
              />
              <FormControl fullWidth>
                <InputLabel id="schedule-start-home-label">{startHomeLabel}</InputLabel>
                <Select
                  labelId="schedule-start-home-label"
                  label={startHomeLabel}
                  value={draft.anchorSide}
                  onChange={(event) => updateDraft({ ...draft, anchorSide: event.target.value as ScheduleSide })}
                >
                  <MenuItem value="A">{family.sideALabel}</MenuItem>
                  <MenuItem value="B">{family.sideBLabel}</MenuItem>
                </Select>
                {advanced && <FormHelperText>Förhandsvisningen visar vilket hem barnet faktiskt börjar hos när schemat träder i kraft.</FormHelperText>}
              </FormControl>
            </Stack>
          </Box>

          <Accordion expanded={advanced} onChange={(_, expanded) => {
            setAdvanced(expanded);
            if (!expanded) updateDraft({ ...draft, anchorDate: draft.effectiveFrom });
          }} disableGutters elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 1 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box>
                <Typography fontWeight={700}>Avancerat: anpassa mönstrets ankare</Typography>
                <Typography variant="body2" color="text.secondary">Använd bara detta om mönstret ska räknas från ett annat datum än dagen då den nya versionen börjar.</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <TextField
                type="date"
                label="Mönstret räknas från"
                helperText="På detta datum är barnet hos det valda starthemmet. Selma räknar sedan mönstret framåt till schemats startdag."
                slotProps={{ inputLabel: { shrink: true } }}
                value={draft.anchorDate}
                onChange={(event) => updateDraft({ ...draft, anchorDate: event.target.value })}
                fullWidth
              />
            </AccordionDetails>
          </Accordion>

          {draft.template === "PrimaryAlternateWeekends" && (
            <Box>
              <Typography fontWeight={700} gutterBottom>3. Anpassa helgen</Typography>
              <Stack spacing={2}>
                <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
                  <FormControl fullWidth>
                    <InputLabel id="schedule-weekend-start-label">Helgen börjar</InputLabel>
                    <Select
                      labelId="schedule-weekend-start-label"
                      label="Helgen börjar"
                      value={draft.parameters.weekendStartsOn}
                      onChange={(event) => updateDraft({ ...draft, parameters: { ...draft.parameters, weekendStartsOn: Number(event.target.value) } })}
                    >
                      {weekdayOptions.map((day) => <MenuItem key={day.value} value={day.value}>{day.label}</MenuItem>)}
                    </Select>
                  </FormControl>
                  <TextField
                    type="number"
                    label="Antal dagar per helg"
                    slotProps={{ htmlInput: { min: 1, max: 7 } }}
                    value={draft.parameters.weekendLengthDays}
                    onChange={(event) => updateDraft({ ...draft, parameters: { ...draft.parameters, weekendLengthDays: Number(event.target.value) } })}
                    fullWidth
                  />
                </Stack>
                <FormControl fullWidth>
                  <InputLabel id="schedule-recurring-weekday-label">Återkommande vardagsbesök</InputLabel>
                  <Select
                    labelId="schedule-recurring-weekday-label"
                    label="Återkommande vardagsbesök"
                    value={draft.parameters.recurringWeekday ?? ""}
                    onChange={(event) => updateDraft({ ...draft, parameters: { ...draft.parameters, recurringWeekday: String(event.target.value) === "" ? null : Number(event.target.value) } })}
                  >
                    <MenuItem value="">Inget</MenuItem>
                    {weekdayOptions.slice(0, 5).map((day) => <MenuItem key={day.value} value={day.value}>{day.label}</MenuItem>)}
                  </Select>
                </FormControl>
                <FormControlLabel
                  control={<Checkbox checked={draft.parameters.recurringWeekdayOvernight} disabled={draft.parameters.recurringWeekday == null} onChange={(event) => updateDraft({ ...draft, parameters: { ...draft.parameters, recurringWeekdayOvernight: event.target.checked } })} />}
                  label="Vardagsbesöket är en övernattning"
                />
              </Stack>
            </Box>
          )}

          {family.exchangeDetailLevel !== "Day" && (
            <TextField
              type="time"
              label="Bytestid"
              helperText="Tiden visas på de dagar då barnet byter hem."
              slotProps={{ inputLabel: { shrink: true } }}
              value={draft.changeoverTime ?? ""}
              onChange={(event) => updateDraft({ ...draft, changeoverTime: event.target.value || null })}
            />
          )}
          {family.exchangeDetailLevel === "DayTimeAndPlace" && (
            <TextField label="Bytesplats" value={draft.changeoverPlace ?? ""} onChange={(event) => updateDraft({ ...draft, changeoverPlace: event.target.value || null })} />
          )}

          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <Button variant="outlined" disabled={busy || !draft.effectiveFrom || !draft.anchorDate} onClick={() => void previewDraft()}>
              {busy && !preview ? <CircularProgress size={20} /> : "Förhandsvisa sex veckor"}
            </Button>
            <Button variant="contained" disabled={busy || !preview} onClick={() => void activate()}>
              Aktivera schema
            </Button>
          </Stack>

          {preview && <SchedulePreview family={family} draft={draft} preview={preview} />}

          {showHistory && (
            <Box>
              <Typography variant="h6">Historik</Typography>
              {versions.length === 0
                ? <Typography color="text.secondary">Inget schema är aktiverat.</Typography>
                : versions.map((version) => <Typography key={version.id}>{scheduleTemplates[version.template].name} · från {version.effectiveFrom}</Typography>)}
            </Box>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}
