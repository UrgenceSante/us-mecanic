import { Link } from "@mui/material";

/** Lien d'évitement, visible uniquement au focus clavier, vers le contenu principal (WCAG 2.4.1). */
export default function SkipLink({ targetId = "main" }: { targetId?: string }) {
  return (
    <Link
      href={`#${targetId}`}
      onClick={(e) => {
        // Le routeur est en HashRouter : on déplace le focus sans changer l'URL.
        e.preventDefault();
        const target = document.getElementById(targetId);
        target?.focus();
        target?.scrollIntoView();
      }}
      sx={{
        position: "absolute",
        left: 8,
        top: -100,
        zIndex: (t) => t.zIndex.tooltip + 1,
        bgcolor: "background.paper",
        color: "text.primary",
        px: 2,
        py: 1,
        borderRadius: 1,
        "&:focus": { top: 8 },
      }}
    >
      Aller au contenu
    </Link>
  );
}
