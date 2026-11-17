import { Box, Button } from "@mui/material";
import { useNavigate } from "react-router";
import PageTitle from "../shared/components/PageTitle";

export default function Page404() {
  const navigate = useNavigate();
  return (
    <Box
      component="main"
      id="main"
      sx={{
        margin: "auto",
        padding: 2,
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      <PageTitle sx={{ textAlign: "center" }}>Page introuvable</PageTitle>
      <Button onClick={() => navigate("/")} variant="text">
        Retour à la page principale
      </Button>
    </Box>
  );
}
