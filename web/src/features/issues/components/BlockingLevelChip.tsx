import AlarmIcon from "@mui/icons-material/Alarm";
import BlockIcon from "@mui/icons-material/Block";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";
import type { ReactElement } from "react";
import StatusChip, { type StatusTone } from "../../../shared/components/StatusChip";
import { blockingLevelLabels } from "../types/labels";
import type { BlockingLevel } from "../types/problem";

const blockingLevelChips: Record<BlockingLevel, { tone: StatusTone; icon: ReactElement }> = {
  Immobilizing: { tone: "error", icon: <BlockIcon /> },
  Unanalyzed: { tone: "warning", icon: <HelpOutlineIcon /> },
  OverdueAction: { tone: "warning", icon: <AlarmIcon /> },
  Other: { tone: "neutral", icon: <InfoOutlinedIcon /> },
};

export default function BlockingLevelChip({ level }: { level: BlockingLevel }) {
  return <StatusChip {...blockingLevelChips[level]} category="Niveau" label={blockingLevelLabels[level]} />;
}
