import { setWorkerUrl } from "maplibre-gl";
// MapLibre 6 charge son worker via une URL relative au module, que Vite ne bundle pas :
// on le fait bundler explicitement et on déclare son URL.
import workerUrl from "maplibre-gl/dist/maplibre-gl-worker.mjs?worker&url";

setWorkerUrl(workerUrl);
