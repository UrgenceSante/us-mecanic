import "./App.css";
import { HashRouter, Route, Routes } from "react-router";
import Home from "./pages/Home";
import AnalyseDetail from "./pages/AnalyseDetail";
import SubscriptionContainer from "./components/Subscription/SubscriptionContainer";
import GeolocPage from "./pages/GeolocPage";
import AppLayout from "./pages/AppLayout";
import RequireAuth from "./Keycloak/RequireAuth";
import EmbedPage from "./pages/EmbedPage";
import { Box } from "@mui/material";
import VersionDisplay from "./components/Utils/VersionDisplay";
import Page404 from "./pages/Page404";

function App() {
  return (
    <Box height={"100%"} display={"flex"} flexDirection={"column"}>
      <Box flex={1}>
        <HashRouter>
          {/* Routes AVEC AppBar */}
          <Routes>
            <Route
              path="/"
              element={
                <RequireAuth>
                  <AppLayout />
                </RequireAuth>
              }
            >
              <Route index element={<Home />} />
              <Route path="/analyse/:logId" element={<AnalyseDetail />} />
              <Route
                path="/notification-subscriptions"
                element={<SubscriptionContainer />}
              />
              <Route path="/geoloc/:immat/:tripId" element={<GeolocPage />} />
            </Route>

            {/* Routes SANS AppBar */}
            <Route
              path="/embed/geoloc/:immat/:tripId"
              element={
                <EmbedPage>
                  <GeolocPage />
                </EmbedPage>
              }
            />
            <Route path="*" element={<Page404 />} />
          </Routes>
        </HashRouter>
      </Box>
      <VersionDisplay />
    </Box>
  );
}

export default App;
