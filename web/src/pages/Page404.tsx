import { Button, Container, Typography } from "@mui/material";
import { useNavigate } from "react-router";

export default function Page404() {
  const navigate = useNavigate();
  return (
    <Container
      sx={{
        margin: "auto",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      <Typography textAlign={"center"} variant="h5">
        404 Not found
      </Typography>
      <Button
        sx={{ textAlign: "center" }}
        onClick={() => navigate("/")}
        variant="text"
      >
        Retour à la page principale
      </Button>
    </Container>
  );
}
