import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "maplibre-gl/dist/maplibre-gl.css";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "./api/queryClient.ts";
import { SnackbarProvider } from "notistack";
import CustomKeycloakProvider from "./Keycloak/CustomKeycloakProvider.tsx";

createRoot(document.getElementById("root")!).render(
  // <StrictMode>
  <CustomKeycloakProvider>
    <QueryClientProvider client={queryClient}>
      <SnackbarProvider>
        <App />
      </SnackbarProvider>
    </QueryClientProvider>
  </CustomKeycloakProvider>

  // </StrictMode>
);
