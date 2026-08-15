import { useCallback, useEffect, useState } from "react";
import {
  Alert,
  Box,
  Card,
  CardContent,
  Grid,
  Skeleton,
  Typography,
} from "@mui/material";
import { useCalendar } from "../context/CalendarContext";
import { useConfig } from "../context/ConfigContext";
import { apiGet } from "../api/client";
import type { StatisticsDto } from "../api/types";

/** Color tokens for each stat card. */
const CARD_COLORS = {
  parentA: "#1976d2",
  parentB: "#388e3c",
  vab: "#f57c00",
  comments: "#0288d1",
  unassigned: "#757575",
} as const;

interface StatCard {
  label: string;
  value: number;
  color: string;
}

/** Fetches and displays yearly statistics as a responsive card grid. */
export default function Statistics() {
  const { currentYear } = useCalendar();
  const { parentNames } = useConfig();
  const [data, setData] = useState<StatisticsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStats = useCallback(
    async (year: number) => {
      setLoading(true);
      setError(null);
      try {
        const stats = await apiGet<StatisticsDto>(`/api/statistics/${year}`);
        setData(stats);
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    },
    [],
  );

  useEffect(() => {
    void fetchStats(currentYear);
  }, [currentYear, fetchStats]);

  const cards: StatCard[] = data
    ? [
        {
          label: parentNames.parentAName,
          value: data.parentADays,
          color: CARD_COLORS.parentA,
        },
        {
          label: parentNames.parentBName,
          value: data.parentBDays,
          color: CARD_COLORS.parentB,
        },
        { label: "VAB", value: data.vabDays, color: CARD_COLORS.vab },
        {
          label: "Kommentarer",
          value: data.daysWithComments,
          color: CARD_COLORS.comments,
        },
        {
          label: "Ej tilldelade",
          value: data.unassignedDays,
          color: CARD_COLORS.unassigned,
        },
      ]
    : [];

  return (
    <Box data-testid="statistics" sx={{ mt: 3 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Grid container spacing={2}>
        {loading
          ? Array.from({ length: 5 }).map((_, i) => (
              <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }} key={i}>
                <Card variant="outlined">
                  <CardContent sx={{ textAlign: "center" }}>
                    <Skeleton variant="text" width="60%" sx={{ mx: "auto", fontSize: "2rem" }} />
                    <Skeleton variant="text" width="80%" sx={{ mx: "auto" }} />
                  </CardContent>
                </Card>
              </Grid>
            ))
          : cards.map((card) => (
              <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }} key={card.label}>
                <Card
                  variant="outlined"
                  sx={{ borderTop: `4px solid ${card.color}` }}
                >
                  <CardContent sx={{ textAlign: "center" }}>
                    <Typography
                      variant="h3"
                      component="h3"
                      sx={{ color: card.color, fontWeight: 700 }}
                    >
                      {card.value}
                    </Typography>
                    <Typography variant="body1" color="text.secondary">
                      {card.label}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
      </Grid>
    </Box>
  );
}
