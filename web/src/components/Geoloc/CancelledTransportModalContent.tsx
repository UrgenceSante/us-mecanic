import { Box, Button, Stack, Typography } from "@mui/material";

interface Props {
  onValidate: () => void;
  onCancel: () => void;
}

function CancelledTransportModalContent(props: Props) {
  const { onValidate, onCancel } = props;

  const style = {
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-50%, -50%)",
    width: 400,
    bgcolor: "background.paper",
    border: "2px solid #000",
    boxShadow: 24,
    pt: 2,
    px: 4,
    pb: 3,
  };

  return (
    <Box sx={{ ...style }}>
      <Typography variant="h6">Aucune arrivée renseignée !</Typography>
      <Typography>Est-ce une sortie blanche ?</Typography>

      <Stack mt={3} direction={"row"} gap={2}>
        <Button onClick={onValidate} variant="contained">
          Oui
        </Button>
        <Button onClick={onCancel} color="error">
          Non
        </Button>
      </Stack>
    </Box>
  );
}

export default CancelledTransportModalContent;
