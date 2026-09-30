import { Button, type ButtonProps } from "@mui/material";
import KeyboardArrowLeftIcon from "@mui/icons-material/KeyboardArrowLeft";
import { useNavigate } from "react-router";
export default function BackButton(props: ButtonProps) {
  const navigate = useNavigate();
  return (
    <Button
      {...props}
      onClick={() => navigate(-1)}
      variant="text"
      startIcon={<KeyboardArrowLeftIcon />}
    >
      Retour
    </Button>
  );
}
