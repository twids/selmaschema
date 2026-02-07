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
  common: {
    cancel: 'Avbryt',
    loading: 'Laddar...',
  },
  calendar: {
    parentA: 'Förälder A',
    parentB: 'Förälder B',
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
  changeRequest: {
    title: 'Bytesförfrågningar',
    createNew: 'Skapa ny bytesförfrågan',
    noRequests: 'Inga bytesförfrågningar',
    requestedBy: 'Begärd av',
    swapLabel: '{{from}} → {{to}}',
    comment: 'Kommentar',
    reviewedBy: 'Granskad av',
    review: 'Granska',
    confirmCancel: 'Är du säker på att du vill avbryta denna förfrågan?',
    tabs: {
      pending: 'Väntande',
      all: 'Alla',
    },
    create: {
      title: 'Skapa bytesförfrågan',
      selectDate: 'Välj datum',
      addDate: 'Lägg till',
      selectedDates: 'Valda datum',
      requestedParent: 'Begär förälder',
      comment: 'Kommentar (valfritt)',
      submit: 'Skicka förfrågan',
    },
    reviewDialog: {
      title: 'Granska bytesförfrågan',
      date: 'Datum',
      requestedBy: 'Begärd av',
      responseComment: 'Ditt svar (valfritt)',
      approve: 'Godkänn',
      reject: 'Avvisa',
    },
    errors: {
      noDates: 'Välj minst ett datum',
      createFailed: 'Kunde inte skapa bytesförfrågan',
    },
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
