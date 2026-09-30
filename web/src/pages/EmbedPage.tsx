import { Box } from "@mui/material";
import type { ReactNode } from "react";

interface EmbedPageProps {
  children: ReactNode;
}

export default function EmbedPage(props: EmbedPageProps) {
  return <Box sx={{ padding: 2, height: "100%" }}>{props.children}</Box>;
}
