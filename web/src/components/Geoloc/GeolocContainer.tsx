import { useRef, useState } from "react";
import GeolocMap from "./GeolocMap";
import useGeolocService from "./useGeolocService";
import type { Point, StopPoint, Trip } from "./Geoloc.model";
import { Box, Button, Modal, Stack, Typography } from "@mui/material";
import SendIcon from "@mui/icons-material/Send";
import { useParams } from "react-router";
import LogoLoader from "../Utils/LogoLoader";
import ErrorHandler from "../Utils/Error/ErrorHandler";
import { time } from "../Utils/DateTime.service";
import useNotifSnack from "../../hooks/useNotifSnack";
import SelectedStopPoint from "./SelectedStopPoint";
import type { MapRef } from "react-map-gl/maplibre";
import MainPoint from "./MainPoint";
import { green, orange } from "@mui/material/colors";
import CancelledTransportModalContent from "./CancelledTransportModalContent";
import { distanceInMeters } from "./geo.utils";

export default function GeolocContainer() {
  const { immat, tripId } = useParams();
  const { notifyError } = useNotifSnack();

  const { getMinMax, query, mutation } = useGeolocService(immat!, tripId!);

  const [selectedPoint, setSelectedPoint] = useState<StopPoint>();
  const [depart, setDepart] = useState<StopPoint>();
  const [arrive, setArrive] = useState<StopPoint>();
  const [openModal, setOpenModal] = useState(false);
  const mapRef = useRef<MapRef>(null);

  const onClick = (p: StopPoint) => setSelectedPoint(p);

  const centerOnPoint = (p: Point) => {
    const map = mapRef.current?.getMap();
    if (!map) return;
    return map.flyTo({
      center: [p.Longitude, p.Latitude],
      zoom: 17,
    });
  };

  const toggleModal = () => setOpenModal(!openModal);

  const handleSetDepart = (p: StopPoint) => {
    if (arrive && time(arrive.StartDatetime) < time(p.StartDatetime))
      return notifyError("le depart ne peut pas succéder l'arrivée");

    setDepart(p);
    if (arrive === p) setArrive(undefined);
    setSelectedPoint(undefined);
  };

  const handleSetArrive = (p: StopPoint) => {
    if (depart && time(depart.StartDatetime) > time(p.StartDatetime))
      return notifyError("L'arrivé ne peut pas preceder le départ");
    setArrive(p);
    if (depart === p) setDepart(undefined);
    setSelectedPoint(undefined);
  };

  if (query.isLoading) return <LogoLoader />;
  if (query.isError) return <ErrorHandler error={query.error} />;
  if (!query.data) return <>No data</>;

  const handleOnCertifClick = () => {
    if (!depart) return;
    if (!arrive) toggleModal();
    else onSend();
  };

  const onSend = () => {
    if (!depart) return;
    const trip: Trip = {
      DrivePoints: arrive //Si pas d'arrivé considéré comme une sortie blanche, donc pas de DrivePoints
        ? query.data.DrivePoints.filter(
            (p) =>
              time(p.LocalTime) > time(depart.StartDatetime) &&
              time(p.LocalTime) < time(arrive.StartDatetime)
          )
        : [],
      // StopPoints: query.data.StopPoints.filter(
      //   (p) =>
      //     time(p.StartDatetime) >= time(depart.StartDatetime) &&
      //     time(p.StartDatetime) <= time(arrive.StartDatetime)
      // ),
      StopPoints: arrive ? [depart, arrive] : [depart],
      Departure: query.data.Departure,
      Arrival: query.data.Arrival,
      TripDescription: query.data.TripDescription,
      TripId: query.data.TripId,
    };
    mutation.mutate(trip);
  };

  const minMax = getMinMax(query.data.DrivePoints, query.data.StopPoints);

  const unsetDepart = () => setDepart(undefined);
  const unsetArrivee = () => setArrive(undefined);

  return (
    <Box display={"flex"} flexDirection={"row"} height={"100%"}>
      <Box flex={4}>
        <GeolocMap
          mapRef={mapRef}
          minMax={minMax}
          depart={depart}
          arrivee={arrive}
          setDepart={(p) => handleSetDepart(p)}
          setArrive={(p) => handleSetArrive(p)}
          onReset={() => setSelectedPoint(undefined)}
          geoloc={query.data}
          onClick={onClick}
          selectedPoint={selectedPoint}
        />
      </Box>

      <Stack gap={1} direction={"column"} flex={1} padding={2}>
        <Typography>Départ</Typography>
        <MainPoint
          title={query.data.Departure.Label}
          point={query.data.Departure}
          color={green[200]}
          onClick={centerOnPoint}
        />
        <SelectedStopPoint
          distance={distanceInMeters(depart, query.data.Departure)}
          onClick={centerOnPoint}
          point={depart}
          title="Départ séléctionné"
          onDelete={unsetDepart}
        />

        <Typography mt={2}>Arrivée</Typography>
        <MainPoint
          title={query.data.Arrival.Label}
          point={query.data.Arrival}
          color={orange[200]}
          onClick={centerOnPoint}
        />

        <SelectedStopPoint
          distance={distanceInMeters(arrive, query.data.Arrival)}
          onClick={centerOnPoint}
          point={arrive}
          title="Arrivée séléctionné"
          onDelete={unsetArrivee}
        />

        <Box flex={1} />
        <Button
          sx={{ marginTop: 1 }}
          onClick={handleOnCertifClick}
          variant="contained"
          startIcon={<SendIcon />}
          disabled={!depart || mutation.isPending}
          loading={mutation.isPending}
        >
          Certifier
        </Button>
      </Stack>
      <Modal open={openModal} onClose={toggleModal}>
        <CancelledTransportModalContent
          onValidate={onSend}
          onCancel={toggleModal}
        />
      </Modal>
    </Box>
  );
}
