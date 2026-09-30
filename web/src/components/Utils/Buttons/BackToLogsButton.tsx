import { Button, type ButtonProps } from "@mui/material";

import { useNavigate } from "react-router";
import KeyboardArrowLeftIcon from "@mui/icons-material/KeyboardArrowLeft";

export default function BackToLogsButton(props: ButtonProps) {
  const navigate = useNavigate();
  return (
    <Button
      {...props}
      onClick={() => navigate("/")}
      variant="text"
      startIcon={<KeyboardArrowLeftIcon />}
    >
      Retour aux logs
    </Button>
  );
}
