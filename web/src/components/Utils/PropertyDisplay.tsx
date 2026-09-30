import { Box, Typography } from "@mui/material";
import type { ReactNode } from "react";

interface PropertyDisplayProps {
  title: string;
  content: ReactNode;
  contentColor?: string;
}
export default function PropertyDisplay(props: PropertyDisplayProps) {
  return (
    <Box>
      <Typography variant="caption">{props.title}</Typography>
      <Typography
        className="fw-bold"
        variant="body1"
        color={props.contentColor || "primary"}
      >
        {props.content}
      </Typography>
    </Box>
  );
}
