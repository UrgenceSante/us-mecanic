import { geoClient } from "../../api/client";
import type { Trip } from "./Geoloc.model";

export const getGeoloc = async (immat: string, tripId: string) => {
  const geolocData = (
    await geoClient.get(`CertifyTrip?gTripId=${tripId}&strImmat=${immat}`)
  ).data;

  // const tripData: Trip = {
  //   Arrival: geolocData.Arrival,
  //   Departure: geolocData.Departure,
  //   DrivePoints: geolocData.DrivePointsModels,
  //   StopPoints: geolocData.StopPointsModel,
  //   TripDescription: geolocData.TripDescription,
  //   TripId: geolocData.TripId,
  // };

  return geolocData;
};

export const postGeoloc = async (trip: Trip) => {
  return (await geoClient.post(`CertifyTrip`, trip)).data;
};
