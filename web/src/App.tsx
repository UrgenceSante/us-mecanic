import "./App.css";
import { HashRouter, Route, Routes } from "react-router";
import DashboardPage from "./features/dashboard/pages/DashboardPage";
import SubscriptionContainer from "./components/Subscription/SubscriptionContainer";
import GeolocPage from "./pages/GeolocPage";
import AppLayout from "./pages/AppLayout";
import RequireAuth from "./Keycloak/RequireAuth";
import EmbedPage from "./pages/EmbedPage";
import { Box } from "@mui/material";
import VersionDisplay from "./components/Utils/VersionDisplay";
import Page404 from "./pages/Page404";
import VehicleListPage from "./features/vehicles/pages/VehicleListPage";
import VehicleDetailPage from "./features/vehicles/pages/VehicleDetailPage";
import VehicleImportPage from "./features/vehicles/pages/VehicleImportPage";
import GaragesPage from "./features/garages/pages/GaragesPage";
import ExpenseCategoriesPage from "./features/costs/pages/ExpenseCategoriesPage";
import CostSummaryPage from "./features/costs/pages/CostSummaryPage";
import ProblemReferencesPage from "./features/problem-references/pages/ProblemReferencesPage";
import VehicleTypesPage from "./features/vehicle-types/pages/VehicleTypesPage";
import DeadlineTypesPage from "./features/deadline-types/pages/DeadlineTypesPage";
import ProblemDetailPage from "./features/issues/pages/ProblemDetailPage";

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
              <Route index element={<DashboardPage />} />
              <Route path="/costs" element={<CostSummaryPage />} />
              <Route path="/vehicles" element={<VehicleListPage />} />
              <Route path="/vehicles/import" element={<VehicleImportPage />} />
              <Route path="/vehicle-types" element={<VehicleTypesPage />} />
              <Route path="/garages" element={<GaragesPage />} />
              <Route path="/expense-categories" element={<ExpenseCategoriesPage />} />
              <Route path="/problem-references" element={<ProblemReferencesPage />} />
              <Route path="/deadline-types" element={<DeadlineTypesPage />} />
              <Route path="/vehicles/:vehicleId" element={<VehicleDetailPage />} />
              <Route path="/problems/:problemId" element={<ProblemDetailPage />} />
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
