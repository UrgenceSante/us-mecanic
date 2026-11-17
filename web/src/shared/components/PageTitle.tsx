import { Typography, type TypographyProps } from "@mui/material";
import useDocumentTitle from "../hooks/useDocumentTitle";

interface PageTitleProps extends Pick<TypographyProps, "flexGrow" | "marginBottom" | "sx"> {
  children: string;
}

/** Titre de page : un seul h1 par écran (rendu h5) et `document.title` aligné. */
export default function PageTitle({ children, ...rest }: PageTitleProps) {
  useDocumentTitle(children);
  return (
    <Typography variant="h5" component="h1" {...rest}>
      {children}
    </Typography>
  );
}
