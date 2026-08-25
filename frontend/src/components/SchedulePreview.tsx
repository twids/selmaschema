import { Alert, Box, Chip, Divider, Stack, Typography } from "@mui/material";
import type { FamilyDto, ScheduleDraft, SchedulePreviewDto } from "../api/v2";
import { homeName, scheduleTemplates } from "./schedulePresentation";

interface SchedulePreviewProps {
  family: FamilyDto;
  draft: ScheduleDraft;
  preview: SchedulePreviewDto;
}

const weekdayHeadings = ["Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];

function parseDate(value: string): Date {
  return new Date(`${value}T12:00:00`);
}

function shortDate(value: string): string {
  return parseDate(value).toLocaleDateString("sv-SE", { weekday: "short", day: "numeric", month: "short" });
}

function shortTime(value: string | null): string | null {
  return value ? value.slice(0, 5) : null;
}

export default function SchedulePreview({ family, draft, preview }: SchedulePreviewProps) {
  const firstDay = preview.days[0];
  const firstTwoWeeks = preview.days.slice(0, 14);
  const homeADays = firstTwoWeeks.filter((day) => day.side === "A").length;
  const homeBDays = firstTwoWeeks.filter((day) => day.side === "B").length;
  const changes = preview.days.flatMap((day, index) => {
    const previous = preview.days[index - 1];
    return index > 0 && day.side !== null && previous?.side !== day.side ? [day] : [];
  });
  const leadingBlanks = firstDay ? (parseDate(firstDay.date).getDay() + 6) % 7 : 0;
  const metadata = scheduleTemplates[draft.template];
  const startsAt = firstDay ? homeName(firstDay.side, family.sideALabel, family.sideBLabel) : "–";
  const anchorHome = draft.anchorSide === "A" ? family.sideALabel : family.sideBLabel;
  const otherHome = draft.anchorSide === "A" ? family.sideBLabel : family.sideALabel;

  return (
    <Stack spacing={2} data-testid="schedule-preview">
      <Alert severity="success">
        <Typography fontWeight={700}>Förhandsvisningen är redo</Typography>
        <Typography variant="body2">
          Aktivera schemat inom 15 minuter. Ändrar du någon inställning behöver du förhandsvisa igen.
        </Typography>
      </Alert>

      <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", md: "minmax(0, 2fr) minmax(260px, 1fr)" }, gap: 2 }}>
        <Box>
          <Typography variant="h6">Så här blir schemat</Typography>
          <Typography>{metadata.description(anchorHome, otherHome)}</Typography>
          <Typography color="text.secondary" sx={{ mt: 0.5 }}>
            Start {firstDay ? shortDate(firstDay.date) : draft.effectiveFrom} hos {startsAt}.
          </Typography>
        </Box>
        <Box sx={{ bgcolor: "action.hover", borderRadius: 2, p: 2 }}>
          <Typography fontWeight={700}>Första två veckorna</Typography>
          <Typography>{family.sideALabel}: {homeADays} dagar</Typography>
          <Typography>{family.sideBLabel}: {homeBDays} dagar</Typography>
        </Box>
      </Box>

      <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap" aria-label="Teckenförklaring">
        <Chip label={family.sideALabel} sx={{ bgcolor: "rgba(25, 118, 210, 0.14)", border: "1px solid", borderColor: "primary.main" }} />
        <Chip label={family.sideBLabel} sx={{ bgcolor: "rgba(156, 39, 176, 0.14)", border: "1px solid", borderColor: "secondary.main" }} />
        <Chip label="↪ Byte" variant="outlined" />
        {preview.days.some((day) => day.hasRecurringVisit) && <Chip label="Besök" variant="outlined" />}
      </Stack>

      {changes.length > 0 && (
        <Box>
          <Typography fontWeight={700} gutterBottom>Kommande byten</Typography>
          <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
            {changes.slice(0, 6).map((day) => {
              const detail = [shortTime(day.changeoverTime), day.changeoverPlace].filter(Boolean).join(" · ");
              return <Chip key={day.date} label={`${shortDate(day.date)} → ${homeName(day.side, family.sideALabel, family.sideBLabel)}${detail ? ` · ${detail}` : ""}`} variant="outlined" />;
            })}
          </Stack>
        </Box>
      )}

      <Divider />
      <Typography variant="h6">De första sex veckorna</Typography>
      <Box sx={{ overflowX: "auto", pb: 1 }}>
        <Box role="grid" aria-label="Förhandsvisning av boendeschema" sx={{ minWidth: 720, display: "grid", gridTemplateColumns: "repeat(7, minmax(92px, 1fr))", gap: 1 }}>
          {weekdayHeadings.map((heading) => (
            <Typography key={heading} role="columnheader" fontWeight={700} textAlign="center" color="text.secondary">{heading}</Typography>
          ))}
          {Array.from({ length: leadingBlanks }, (_, index) => <Box key={`blank-${index}`} aria-hidden="true" />)}
          {preview.days.map((day, index) => {
            const previous = preview.days[index - 1];
            const isChange = index > 0 && day.side !== null && previous?.side !== day.side;
            const label = homeName(day.side, family.sideALabel, family.sideBLabel);
            return (
              <Box
                key={day.date}
                role="gridcell"
                aria-label={`${shortDate(day.date)}, ${label}${isChange ? ", byte" : ""}${day.hasRecurringVisit ? ", besök" : ""}`}
                sx={{
                  minHeight: 104,
                  border: "1px solid",
                  borderColor: day.side === "A" ? "primary.main" : day.side === "B" ? "secondary.main" : "divider",
                  bgcolor: day.side === "A" ? "rgba(25, 118, 210, 0.10)" : day.side === "B" ? "rgba(156, 39, 176, 0.10)" : "background.paper",
                  borderRadius: 1.5,
                  p: 1,
                }}
              >
                <Typography fontWeight={700}>{parseDate(day.date).toLocaleDateString("sv-SE", { day: "numeric", month: "short" })}</Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>{label}</Typography>
                {isChange && <Typography variant="caption" display="block" fontWeight={700}>↪ Byte</Typography>}
                {isChange && shortTime(day.changeoverTime) && <Typography variant="caption" display="block">Kl. {shortTime(day.changeoverTime)}</Typography>}
                {isChange && day.changeoverPlace && <Typography variant="caption" display="block">{day.changeoverPlace}</Typography>}
                {day.hasRecurringVisit && <Typography variant="caption" display="block" fontWeight={700}>Besök</Typography>}
              </Box>
            );
          })}
        </Box>
      </Box>
    </Stack>
  );
}
