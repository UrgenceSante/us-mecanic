import { Chip } from "@mui/material";
import { blockingLevelColors, blockingLevelLabels } from "../types/labels";
import type { BlockingLevel } from "../types/problem";

export default function BlockingLevelChip({ level }: { level: BlockingLevel }) {
  return <Chip size="small" color={blockingLevelColors[level]} label={blockingLevelLabels[level]} />;
}
