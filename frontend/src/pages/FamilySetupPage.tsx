import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, FormControl, FormControlLabel, FormHelperText,
  FormLabel, InputLabel, MenuItem, Radio, RadioGroup, Select, Stack, Step, StepLabel, Stepper, TextField,
  Typography, useMediaQuery,
} from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  familyApi, type ChildDto, type CreatedInvitationDto, type FamilyDto, type ResidenceCalendarDto,
} from "../api/v2";
import type { ScheduleSide } from "../auth/AuthContext";
import { useAuth } from "../auth/AuthContext";
import ScheduleEditor from "../components/ScheduleEditor";

const steps = ["Familjen och hemmen", "Barn och kalender", "Boendeschema", "Bjud in (valfritt)", "Klart"];
const onboardingAuditReason = "Initial familjekonfiguration";

function firstIncompleteStep(family: FamilyDto): number {
  if (family.mySide === null) return 0;
  if (family.activeChildren === 0) return 1;
  if (!family.hasActiveSchedule) return 2;
  return 4;
}

export default function FamilySetupPage() {
  const { familyId = "" } = useParams();
  const navigate = useNavigate();
  const { refresh } = useAuth();
  const theme = useTheme();
  const mobile = useMediaQuery(theme.breakpoints.down("md"));
  const initializedStep = useRef(false);
  const [family, setFamily] = useState<FamilyDto | null>(null);
  const [children, setChildren] = useState<ChildDto[]>([]);
  const [calendars, setCalendars] = useState<ResidenceCalendarDto[]>([]);
  const [activeStep, setActiveStep] = useState(0);
  const [selectedSide, setSelectedSide] = useState<ScheduleSide | "">("");
  const [childName, setChildName] = useState("");
  const [childCalendarId, setChildCalendarId] = useState("");
  const [invitationPermission, setInvitationPermission] = useState<"Editor" | "Viewer">("Editor");
  const [invitationSide, setInvitationSide] = useState<ScheduleSide | "">("");
  const [emailHint, setEmailHint] = useState("");
  const [createdInvitation, setCreatedInvitation] = useState<CreatedInvitationDto | null>(null);
  const [copied, setCopied] = useState<"link" | "code" | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const reload = useCallback(async () => {
    const [nextFamily, nextChildren, nextCalendars] = await Promise.all([
      familyApi.get(familyId),
      familyApi.children(familyId),
      familyApi.calendars(familyId),
    ]);
    setFamily(nextFamily);
    setChildren(nextChildren);
    setCalendars(nextCalendars);
    setSelectedSide((current) => current || nextFamily.mySide || "");
    setChildCalendarId((current) => current || nextCalendars.find((calendar) => calendar.isActive && calendar.name === "Boendeschema")?.id || nextCalendars.find((calendar) => calendar.isActive)?.id || "");
    setInvitationSide((current) => current || (nextFamily.mySide === "A" ? "B" : nextFamily.mySide === "B" ? "A" : ""));
    if (!initializedStep.current) {
      setActiveStep(firstIncompleteStep(nextFamily));
      initializedStep.current = true;
    }
  }, [familyId]);

  useEffect(() => {
    setLoading(true);
    void reload()
      .catch(() => setError("Familjens konfiguration kunde inte laddas."))
      .finally(() => setLoading(false));
  }, [reload]);

  if (loading) return <CircularProgress aria-label="Laddar familjeguiden" />;
  if (!family) return <Alert severity="error">Familjen kunde inte laddas.</Alert>;
  if (family.myPermission !== "Owner") return <Alert severity="warning">Bara familjens ägare kan använda familjeguiden.</Alert>;

  const activeChildren = children.filter((child) => child.isActive);
  const activeCalendars = calendars.filter((calendar) => calendar.isActive);
  const defaultCalendar = activeCalendars.find((calendar) => calendar.name === "Boendeschema") ?? activeCalendars[0];
  const homesValid = family.name.trim() && family.sideALabel.trim() && family.sideBLabel.trim() &&
    family.sideALabel.trim().toLocaleLowerCase("sv-SE") !== family.sideBLabel.trim().toLocaleLowerCase("sv-SE") &&
    family.timeZoneId.trim() && selectedSide;

  const saveHomes = async () => {
    if (!homesValid) return;
    setBusy(true);
    setError(null);
    try {
      const updated = await familyApi.update(familyId, {
        name: family.name,
        timeZoneId: family.timeZoneId,
        sideALabel: family.sideALabel,
        sideBLabel: family.sideBLabel,
        exchangeDetailLevel: family.exchangeDetailLevel,
      });
      await familyApi.updateMember(familyId, family.myMemberId, family.myPermission, selectedSide as ScheduleSide, onboardingAuditReason);
      setFamily({ ...updated, mySide: selectedSide as ScheduleSide });
      await refresh();
      setActiveStep(1);
    } catch {
      setError("Familjen och hemmen kunde inte sparas. Kontrollera uppgifterna och försök igen.");
    } finally {
      setBusy(false);
    }
  };

  const addChild = async () => {
    if (!childName.trim() || !childCalendarId) return;
    setBusy(true);
    setError(null);
    try {
      await familyApi.createChild(familyId, childName, childCalendarId);
      setChildName("");
    } catch {
      setError("Barnet kunde inte läggas till. Kontrollera namnet och kalendern.");
      setBusy(false);
      return;
    }
    try {
      await reload();
    } catch {
      setError("Barnet lades till, men sidan kunde inte uppdateras. Ladda om sidan för att fortsätta.");
    } finally {
      setBusy(false);
    }
  };

  const createInvitation = async () => {
    setBusy(true);
    setError(null);
    try {
      setCreatedInvitation(await familyApi.createInvitation(
        familyId,
        invitationPermission,
        invitationSide || null,
        emailHint,
      ));
    } catch {
      setError("Inbjudan kunde inte skapas. Försök igen.");
    } finally {
      setBusy(false);
    }
  };

  const copy = async (kind: "link" | "code", value: string) => {
    try {
      await navigator.clipboard.writeText(value);
      setCopied(kind);
    } catch {
      setError("Värdet kunde inte kopieras automatiskt. Markera och kopiera det manuellt.");
    }
  };

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" alignItems={{ sm: "center" }} spacing={1}>
        <Box>
          <Typography variant="h3" fontWeight={700}>Kom igång med {family.name}</Typography>
          <Typography color="text.secondary">Vi tar en tydlig del i taget. Allt sparas efter varje steg.</Typography>
        </Box>
        <Button component={Link} to={`/families/${familyId}`}>Fortsätt senare</Button>
      </Stack>

      <Stepper activeStep={activeStep} orientation={mobile ? "vertical" : "horizontal"} alternativeLabel={!mobile}>
        {steps.map((label) => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}
      </Stepper>

      {error && <Alert severity="error" onClose={() => setError(null)}>{error}</Alert>}

      {activeStep === 0 && (
        <Card><CardContent><Stack spacing={3}>
          <Box>
            <Typography variant="h5">1. Familjen och hemmen</Typography>
            <Typography color="text.secondary">Hemmen beskriver två bostäder, inte två användarkonton. Flera vuxna kan tillhöra samma hem.</Typography>
          </Box>
          <TextField label="Familjens namn" value={family.name} onChange={(event) => setFamily({ ...family, name: event.target.value })} />
          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <TextField fullWidth label="Första hemmet" helperText="Till exempel Hos Tomas" value={family.sideALabel} onChange={(event) => setFamily({ ...family, sideALabel: event.target.value })} />
            <TextField fullWidth label="Andra hemmet" helperText="Till exempel Hos Anna" value={family.sideBLabel} onChange={(event) => setFamily({ ...family, sideBLabel: event.target.value })} />
          </Stack>
          {family.sideALabel.trim().toLocaleLowerCase("sv-SE") === family.sideBLabel.trim().toLocaleLowerCase("sv-SE") && (
            <Alert severity="warning">Hemmen behöver olika namn för att kalendern ska bli begriplig.</Alert>
          )}
          <FormControl required>
            <FormLabel>Vilket hem tillhör du?</FormLabel>
            <RadioGroup row value={selectedSide} onChange={(event) => setSelectedSide(event.target.value as ScheduleSide)}>
              <FormControlLabel value="A" control={<Radio />} label={family.sideALabel || "Första hemmet"} />
              <FormControlLabel value="B" control={<Radio />} label={family.sideBLabel || "Andra hemmet"} />
            </RadioGroup>
            <FormHelperText>Det påverkar inte din behörighet och kan ändras senare.</FormHelperText>
          </FormControl>
          <TextField label="Tidszon" helperText="Används för datumgränser. Standard för Sverige är Europe/Stockholm." value={family.timeZoneId} onChange={(event) => setFamily({ ...family, timeZoneId: event.target.value })} />
          <FormControl>
            <FormLabel>Vad ska visas vid ett byte?</FormLabel>
            <RadioGroup value={family.exchangeDetailLevel} onChange={(event) => setFamily({ ...family, exchangeDetailLevel: event.target.value as FamilyDto["exchangeDetailLevel"] })}>
              <FormControlLabel value="Day" control={<Radio />} label="Endast bytesdag" />
              <FormControlLabel value="DayAndTime" control={<Radio />} label="Bytesdag och klockslag" />
              <FormControlLabel value="DayTimeAndPlace" control={<Radio />} label="Bytesdag, klockslag och plats" />
            </RadioGroup>
          </FormControl>
          <Box><Button variant="contained" disabled={busy || !homesValid} onClick={() => void saveHomes()}>Spara och fortsätt</Button></Box>
        </Stack></CardContent></Card>
      )}

      {activeStep === 1 && (
        <Card><CardContent><Stack spacing={3}>
          <Box>
            <Typography variant="h5">2. Barn och kalender</Typography>
            <Typography color="text.secondary">Barn som följer samma boendemönster kan dela kalendern {defaultCalendar?.name ?? "Boendeschema"}. Fler kalendrar kan läggas till senare om barnen har olika scheman.</Typography>
          </Box>
          {activeChildren.length > 0 && (
            <Stack spacing={1}>
              {activeChildren.map((child) => <Alert severity="success" key={child.id}>{child.displayName} är tillagd i {calendars.find((calendar) => calendar.id === child.calendarId)?.name ?? "en kalender"}.</Alert>)}
            </Stack>
          )}
          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <TextField fullWidth label="Barnets visningsnamn" value={childName} onChange={(event) => setChildName(event.target.value)} />
            {activeCalendars.length > 1 && (
              <FormControl fullWidth>
                <InputLabel id="setup-child-calendar-label">Kalender</InputLabel>
                <Select labelId="setup-child-calendar-label" label="Kalender" value={childCalendarId} onChange={(event) => setChildCalendarId(event.target.value)}>
                  {activeCalendars.map((calendar) => <MenuItem key={calendar.id} value={calendar.id}>{calendar.name}</MenuItem>)}
                </Select>
              </FormControl>
            )}
            <Button variant="outlined" disabled={busy || !childName.trim() || !childCalendarId} onClick={() => void addChild()}>Lägg till barn</Button>
          </Stack>
          <Stack direction="row" justifyContent="space-between">
            <Button onClick={() => setActiveStep(0)}>Tillbaka</Button>
            <Button variant="contained" disabled={activeChildren.length === 0} onClick={() => setActiveStep(2)}>Nästa: boendeschema</Button>
          </Stack>
        </Stack></CardContent></Card>
      )}

      {activeStep === 2 && (
        <Stack spacing={2}>
          <Box>
            <Typography variant="h5">3. Boendeschema</Typography>
            <Typography color="text.secondary">Välj ett mönster och granska sex riktiga kalenderveckor innan du aktiverar det.</Typography>
          </Box>
          {defaultCalendar ? (
            <ScheduleEditor
              family={family}
              calendarId={defaultCalendar.id}
              showHistory={false}
              onActivated={async () => {
                await reload();
                setActiveStep(3);
              }}
            />
          ) : <Alert severity="error">Det finns ingen aktiv boendekalender för familjen.</Alert>}
          <Box><Button onClick={() => setActiveStep(1)}>Tillbaka</Button></Box>
        </Stack>
      )}

      {activeStep === 3 && (
        <Card><CardContent><Stack spacing={3}>
          <Box>
            <Typography variant="h5">4. Bjud in en annan vuxen</Typography>
            <Typography color="text.secondary">Detta steg är valfritt. Inbjudan ger både en länk och en kort engångskod som gäller i sju dagar.</Typography>
          </Box>
          {!createdInvitation && (
            <>
              <FormControl>
                <InputLabel id="setup-invitation-permission-label">Behörighet</InputLabel>
                <Select labelId="setup-invitation-permission-label" label="Behörighet" value={invitationPermission} onChange={(event) => setInvitationPermission(event.target.value as "Editor" | "Viewer")}>
                  <MenuItem value="Editor">Kan redigera kalendern</MenuItem>
                  <MenuItem value="Viewer">Kan bara läsa</MenuItem>
                </Select>
              </FormControl>
              <FormControl>
                <InputLabel id="setup-invitation-home-label">Tillhör hem</InputLabel>
                <Select labelId="setup-invitation-home-label" label="Tillhör hem" value={invitationSide} onChange={(event) => setInvitationSide(event.target.value as ScheduleSide | "")}>
                  <MenuItem value="">Inget hem</MenuItem>
                  <MenuItem value="A">{family.sideALabel}</MenuItem>
                  <MenuItem value="B">{family.sideBLabel}</MenuItem>
                </Select>
              </FormControl>
              <TextField label="E-postledtråd (valfri)" helperText="Adressen är bara en ledtråd. Mottagaren bekräftar sin verifierade adress när inbjudan löses in." value={emailHint} onChange={(event) => setEmailHint(event.target.value)} />
              <Box><Button variant="contained" disabled={busy} onClick={() => void createInvitation()}>Skapa inbjudan</Button></Box>
            </>
          )}
          {createdInvitation && (
            <Alert severity="success">
              <Stack spacing={1.5}>
                <Typography fontWeight={700}>Inbjudan är skapad – kopiera den nu</Typography>
                <Typography sx={{ wordBreak: "break-all" }}>{createdInvitation.link}</Typography>
                <Button startIcon={<ContentCopyIcon />} onClick={() => void copy("link", createdInvitation.link)}>{copied === "link" ? "Länk kopierad" : "Kopiera länk"}</Button>
                <Typography variant="h5">{createdInvitation.code}</Typography>
                <Button startIcon={<ContentCopyIcon />} onClick={() => void copy("code", createdInvitation.code)}>{copied === "code" ? "Kod kopierad" : "Kopiera kod"}</Button>
              </Stack>
            </Alert>
          )}
          <Stack direction="row" justifyContent="space-between">
            <Button onClick={() => setActiveStep(2)}>Tillbaka</Button>
            <Button variant={createdInvitation ? "contained" : "outlined"} onClick={() => setActiveStep(4)}>{createdInvitation ? "Fortsätt" : "Hoppa över"}</Button>
          </Stack>
        </Stack></CardContent></Card>
      )}

      {activeStep === 4 && (
        <Card><CardContent><Stack spacing={3} alignItems="flex-start">
          <Alert severity="success" sx={{ width: "100%" }}><Typography fontWeight={700}>Familjen är redo att användas</Typography></Alert>
          <Typography variant="h5">{family.name}</Typography>
          <Typography>{family.sideALabel} och {family.sideBLabel}</Typography>
          <Typography>{activeChildren.length} barn · {defaultCalendar?.name ?? "Boendeschema"}</Typography>
          <Typography>Boendeschemat är aktiverat och kan ändras framåt med nya versioner.</Typography>
          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            {defaultCalendar && <Button variant="contained" onClick={() => navigate(`/families/${familyId}/calendars/${defaultCalendar.id}`)}>Öppna kalendern</Button>}
            <Button component={Link} to={`/families/${familyId}`}>Till familjeöversikten</Button>
          </Stack>
        </Stack></CardContent></Card>
      )}
    </Stack>
  );
}
