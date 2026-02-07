import { useCallback, useRef, useState } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Snackbar,
  Alert,
  Stack,
  CircularProgress,
} from "@mui/material";
import FileDownloadIcon from "@mui/icons-material/FileDownload";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import { useCalendar } from "../context/CalendarContext";
import { useConfig } from "../context/ConfigContext";
import { createExportData, downloadAsJson } from "../utils/dataExport";
import type { ExportData } from "../utils/dataExport";
import { parseImportFile } from "../utils/dataImport";

/** Import/Export controls for calendar data. */
export default function ImportExport() {
  const { calendarData, currentYear, currentMonth, updateDay } = useCalendar();
  const { parentNames } = useConfig();

  const fileInputRef = useRef<HTMLInputElement>(null);

  const [confirmOpen, setConfirmOpen] = useState(false);
  const [importData, setImportData] = useState<ExportData | null>(null);
  const [loading, setLoading] = useState(false);

  const [snackOpen, setSnackOpen] = useState(false);
  const [snackMessage, setSnackMessage] = useState("");
  const [snackSeverity, setSnackSeverity] = useState<"success" | "error">(
    "success",
  );

  /* ---------- export ---------- */

  const handleExport = useCallback(() => {
    const data = createExportData(
      calendarData,
      parentNames,
      currentYear,
      currentMonth,
    );
    downloadAsJson(data);
  }, [calendarData, parentNames, currentYear, currentMonth]);

  /* ---------- import ---------- */

  const handleImportClick = useCallback(() => {
    fileInputRef.current?.click();
  }, []);

  const handleFileChange = useCallback(
    async (event: React.ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      if (!file) return;

      const result = await parseImportFile(file);

      // Reset input so the same file can be re-selected
      if (fileInputRef.current) fileInputRef.current.value = "";

      if (!result.success || !result.data) {
        setSnackMessage(result.error ?? "Invalid file");
        setSnackSeverity("error");
        setSnackOpen(true);
        return;
      }

      setImportData(result.data);
      setConfirmOpen(true);
    },
    [],
  );

  const handleConfirm = useCallback(async () => {
    if (!importData) return;
    setConfirmOpen(false);
    setLoading(true);

    try {
      const entries = Object.entries(importData.calendarData);
      for (const [dateKey, day] of entries) {
        const [y, m, d] = dateKey.split("-").map(Number);
        await updateDay(y, m, d, {
          parent: day.parent,
          isVAB: day.isVAB,
          specialStatus: day.specialStatus,
        });
      }
      setSnackMessage("Import lyckades!");
      setSnackSeverity("success");
    } catch (err) {
      setSnackMessage(
        err instanceof Error ? err.message : "Importen misslyckades",
      );
      setSnackSeverity("error");
    } finally {
      setLoading(false);
      setImportData(null);
      setSnackOpen(true);
    }
  }, [importData, updateDay]);

  const handleCancel = useCallback(() => {
    setConfirmOpen(false);
    setImportData(null);
  }, []);

  const dayCount = importData
    ? Object.keys(importData.calendarData).length
    : 0;

  return (
    <Stack
      direction="row"
      spacing={1}
      data-testid="import-export"
      sx={{ my: 1 }}
    >
      <Button
        variant="outlined"
        size="small"
        startIcon={<FileDownloadIcon />}
        onClick={handleExport}
      >
        Export
      </Button>

      <Button
        variant="outlined"
        size="small"
        startIcon={loading ? <CircularProgress size={16} /> : <FileUploadIcon />}
        onClick={handleImportClick}
        disabled={loading}
      >
        Import
      </Button>

      <input
        ref={fileInputRef}
        type="file"
        accept=".json"
        style={{ display: "none" }}
        onChange={handleFileChange}
      />

      {/* Confirmation dialog */}
      <Dialog open={confirmOpen} onClose={handleCancel}>
        <DialogTitle>Bekräfta import</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Importera data för {importData?.year}-{importData?.month}?
            <br />
            {dayCount} dag(ar) kommer att uppdateras.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancel}>Avbryt</Button>
          <Button onClick={handleConfirm} variant="contained">
            Bekräfta
          </Button>
        </DialogActions>
      </Dialog>

      {/* Feedback snackbar */}
      <Snackbar
        open={snackOpen}
        autoHideDuration={4000}
        onClose={() => setSnackOpen(false)}
      >
        <Alert
          severity={snackSeverity}
          onClose={() => setSnackOpen(false)}
          variant="filled"
        >
          {snackMessage}
        </Alert>
      </Snackbar>
    </Stack>
  );
}
