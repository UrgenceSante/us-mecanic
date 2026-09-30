import { Card, CardContent, CardHeader, type CardProps } from "@mui/material";
import { type ReactNode } from "react";

interface SimpleCardProps extends CardProps {
  title: string;
  action?: ReactNode;
  children: ReactNode;
}
export default function SimpleCard(props: SimpleCardProps) {
  const { action, ...otherprops } = props;
  return (
    <Card variant="outlined" sx={{ borderRadius: 0 }} {...otherprops}>
      <CardHeader
        sx={{ alignItems: "center" }}
        title={props.title}
        action={action}
      />
      {/* <Divider /> */}
      <CardContent>{props.children}</CardContent>
    </Card>
  );
}
