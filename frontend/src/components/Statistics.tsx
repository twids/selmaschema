import { Paper, Typography, Grid, Card, CardContent } from '@mui/material';
import { CalendarData, ParentNames } from '../types';

interface StatisticsProps {
  currentYear: number;
  calendarData: CalendarData;
  parentNames: ParentNames;
}

export default function Statistics({
  currentYear,
  calendarData,
  parentNames,
}: StatisticsProps) {
  const calculateStats = () => {
    let parentADays = 0;
    let parentBDays = 0;
    let vabDays = 0;
    let commentedDays = 0;
    let unassignedDays = 0;

    // Calculate total days in the year
    const isLeapYear = (currentYear % 4 === 0 && currentYear % 100 !== 0) || currentYear % 400 === 0;
    const totalDays = isLeapYear ? 366 : 365;

    // Count assigned days
    Object.values(calendarData).forEach((day) => {
      if (day.parent === 'parentA') parentADays++;
      if (day.parent === 'parentB') parentBDays++;
      if (day.isVAB) vabDays++;
      if (day.comment) commentedDays++;
    });

    unassignedDays = totalDays - parentADays - parentBDays;

    return {
      parentADays,
      parentBDays,
      vabDays,
      commentedDays,
      unassignedDays,
    };
  };

  const stats = calculateStats();

  const statItems = [
    { label: `${parentNames.parentA} Days`, value: stats.parentADays, color: '#bbdefb' },
    { label: `${parentNames.parentB} Days`, value: stats.parentBDays, color: '#f8bbd0' },
    { label: 'VAB Days', value: stats.vabDays, color: '#fff9c4' },
    { label: 'Days with Comments', value: stats.commentedDays, color: '#c5e1a5' },
    { label: 'Unassigned Days', value: stats.unassignedDays, color: '#f5f5f5' },
  ];

  return (
    <Paper elevation={2} sx={{ p: 2, mt: 3 }}>
      <Typography variant="h6" gutterBottom>
        Year Statistics
      </Typography>
      <Grid container spacing={2}>
        {statItems.map((item) => (
          <Grid item xs={12} sm={6} md={2.4} key={item.label}>
            <Card sx={{ bgcolor: item.color }}>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" component="div">
                  {item.value}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {item.label}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Paper>
  );
}
