import type { ScheduleSide } from "../auth/AuthContext";
import type { ScheduleTemplate } from "../api/v2";

export interface ScheduleTemplateMetadata {
  name: string;
  description: (homeA: string, homeB: string) => string;
  pattern: ScheduleSide[];
}

const sides = (value: string): ScheduleSide[] => [...value].map((side) => side as ScheduleSide);

export const scheduleTemplates: Record<ScheduleTemplate, ScheduleTemplateMetadata> = {
  AlternatingWeeks: {
    name: "Varannan vecka",
    description: (homeA, homeB) => `Sju dagar hos ${homeA}, sedan sju dagar hos ${homeB}.`,
    pattern: sides("AAAAAAABBBBBBB"),
  },
  TwoTwoThree: {
    name: "2-2-3",
    description: (homeA, homeB) => `Två dagar hos ${homeA}, två hos ${homeB} och tre hos ${homeA}. Nästa vecka blir omvänd.`,
    pattern: sides("AABBAAABBAABBB"),
  },
  TwoTwoFiveFive: {
    name: "2-2-5-5",
    description: (homeA, homeB) => `Två dagar hos vardera hemmet, följt av fem dagar hos ${homeA} och fem hos ${homeB}.`,
    pattern: sides("AABBAAAAABBBBB"),
  },
  ThreeFourFourThree: {
    name: "3-4-4-3",
    description: (homeA, homeB) => `Tre dagar hos ${homeA}, fyra hos ${homeB}, fyra hos ${homeA} och tre hos ${homeB}.`,
    pattern: sides("AAABBBBAAAABBB"),
  },
  PrimaryAlternateWeekends: {
    name: "Primärt boende + varannan helg",
    description: (homeA, homeB) => `Barnet bor främst hos ${homeA} och återkommande helger hos ${homeB}.`,
    pattern: sides("AAAAAAAAAAABBB"),
  },
};

export const weekdayOptions = [
  { value: 1, label: "Måndag" },
  { value: 2, label: "Tisdag" },
  { value: 3, label: "Onsdag" },
  { value: 4, label: "Torsdag" },
  { value: 5, label: "Fredag" },
  { value: 6, label: "Lördag" },
  { value: 0, label: "Söndag" },
];

export function homeName(side: ScheduleSide | null, homeA: string, homeB: string): string {
  if (side === "A") return homeA;
  if (side === "B") return homeB;
  return "Ej tilldelad";
}

export function patternForAnchor(pattern: ScheduleSide[], anchorSide: ScheduleSide): ScheduleSide[] {
  if (anchorSide === "A") return pattern;
  return pattern.map((side) => side === "A" ? "B" : "A");
}

export function localIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}
