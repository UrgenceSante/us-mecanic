import { useMutation, useQuery } from "@tanstack/react-query";
import { getGeoloc, postGeoloc } from "./Geoloc.api";
import type { AxiosError } from "axios";
import type { DrivePoint, StopPoint, Trip } from "./Geoloc.model";
import useNotifSnack from "../../hooks/useNotifSnack";
import { queryClient } from "../../api/queryClient";

export default function useGeolocService(immat: string, tripId: string) {
  const { notifyError, notifySuccess } = useNotifSnack();
  const query = useQuery<Trip, AxiosError>({
    queryKey: ["geoloc", immat, tripId],
    queryFn: () => getGeoloc(immat, tripId),
  });

  const mutation = useMutation({
    mutationKey: ["geoloc"],
    mutationFn: (t: Trip) => postGeoloc(t),
    onError: (err: AxiosError) => {
      notifyError(err.message);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["geoloc", immat, tripId] });
      notifySuccess("Tout est ok");
    },
  });

  const getMinMax = (
    drivePoints: DrivePoint[],
    stopPoints: StopPoint[]
  ): [number, number] => {
    const pointsTimestamps = [
      ...drivePoints.map((p) => new Date(p.LocalTime).getTime()),
      ...stopPoints.map((p) => new Date(p.StartDatetime).getTime()),
    ];
    const min = Math.min(...pointsTimestamps);
    const max = Math.max(...pointsTimestamps);
    return [min, max];
  };

  // const getClusters = (pts: StopPoint[]) => {
  //   const getDistance = (p1: Point, p2: Point) =>
  //     Math.sqrt(
  //       (p2.Latitude - p1.Latitude) ^ (2 + (p2.Longitude - p1.Longitude)) ^ 2
  //     );
  //   pts.forEach((p,i) => {

  //   })
  // };

  return { query, mutation, getMinMax };
}
