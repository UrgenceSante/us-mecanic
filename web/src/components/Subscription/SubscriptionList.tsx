import { Box } from "@mui/material";
import type { Subscription } from "./Subscription";

interface SubscriptionListProps {
  subscriptions: Subscription[];
}
export default function SubscriptionList(props: SubscriptionListProps) {
  return (
    <Box>
      {props.subscriptions.map((sub) => (
        <Box overflow="clip">
          {sub.userId} - {sub.endpoint}
        </Box>
      ))}
    </Box>
  );
}
