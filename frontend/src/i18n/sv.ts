// Swedish localization strings
export const sv = {
  days: {
    short: ['Sön', 'Mån', 'Tis', 'Ons', 'Tor', 'Fre', 'Lör'],
    long: ['Söndag', 'Måndag', 'Tisdag', 'Onsdag', 'Torsdag', 'Fredag', 'Lördag'],
  },
  months: {
    short: ['Jan', 'Feb', 'Mar', 'Apr', 'Maj', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dec'],
    long: [
      'Januari', 'Februari', 'Mars', 'April', 'Maj', 'Juni',
      'Juli', 'Augusti', 'September', 'Oktober', 'November', 'December'
    ],
  },
  vab: 'VAB',
  week: 'Vecka',
  specialStatus: {
    normal: 'Normal',
    preschoolClosed: 'Förskolan stängd',
    holiday: 'Helgdag',
  },
  comments: {
    addComment: 'Lägg till kommentar',
    commentPlaceholder: 'Lägg till kommentar...',
    commentTomas: 'Kommentar Tomas',
    commentCaroline: 'Kommentar Caroline',
    noComments: 'Inga kommentarer',
  },
  actions: {
    save: 'Spara',
    cancel: 'Avbryt',
    delete: 'Ta bort',
    edit: 'Redigera',
  },
  dayModal: {
    assignedTo: 'Tilldelad till:',
    unassigned: 'Ej tilldelad',
    markAsVAB: 'Markera som VAB',
    specialStatus: 'Special status:',
  },
  auth: {
    adminLogin: 'Admin Inloggning',
    password: 'Lösenord',
    signIn: 'Logga in',
    signingIn: 'Loggar in...',
    invalidPassword: 'Ogiltigt administratörslösenord',
    parentsInfo: 'Föräldrar, använd den magiska länk som skickades till dig. Den loggar in dig automatiskt.',
    invalidMagicLink: 'Ogiltig eller utgången magisk länk',
    requestNewLink: 'Vänligen begär en ny inloggningslänk.',
  },
};

export function formatSwedishDate(date: Date): string {
  const day = sv.days.long[date.getDay()];
  const dayNum = date.getDate();
  const month = sv.months.long[date.getMonth()];
  const year = date.getFullYear();
  return `${day} ${dayNum} ${month} ${year}`;
}

export function formatSwedishTime(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
}
