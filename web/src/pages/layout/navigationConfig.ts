export interface NavItem {
  label: string;
  to: string;
  /** Si vrai, seule la route exacte met l'entrée en évidence. */
  end?: boolean;
}

/** Entrées du quotidien, affichées directement dans la barre. */
export const mainNavItems: NavItem[] = [
  { label: "Tableau de bord", to: "/", end: true },
  { label: "Parc", to: "/vehicles" },
  { label: "Échéances", to: "/deadline-types" },
];

/** Référentiels et pages d'administration, regroupés dans un menu. */
export const adminNavItems: NavItem[] = [
  { label: "Types de véhicule", to: "/vehicle-types" },
  { label: "Garages", to: "/garages" },
  { label: "Catégories de dépense", to: "/expense-categories" },
  { label: "Référentiels problèmes", to: "/problem-references" },
  { label: "Abonnements", to: "/notification-subscriptions" },
  {
    label: "Géoloc",
    to: "/geoloc/FS414ZX/bae74f53-8d18-47c8-bd0b-0b9ead081aad",
  },
];

export function isNavItemActive(item: NavItem, pathname: string): boolean {
  if (item.end) return pathname === item.to;
  return pathname === item.to || pathname.startsWith(`${item.to}/`);
}
