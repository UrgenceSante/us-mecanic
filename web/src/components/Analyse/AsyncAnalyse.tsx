import { useParams } from "react-router";
import useGetAnalyseById from "../../hooks/analyse/useGetAnalyseById";
import AsyncComponent from "../Utils/AsyncComponent";
import type { IAnalyse } from "./IAnalyse";
import AnalyseDisplay from "./AnalyseDisplay";
import AnalyseForm from "./AnalyseForm/AnalyseForm";
import { Skeleton } from "@mui/material";

export default function AsyncAnalyse() {
  const { logId } = useParams();

  const request = useGetAnalyseById(logId || "-1");

  const loading = (
    <>
      <Skeleton variant="rectangular" width={"100%"} height={"100%"} />{" "}
      <Skeleton variant="rectangular" width={"100%"} height={"100%"} />
    </>
  );

  return (
    <AsyncComponent
      query={request}
      render={(analyse: IAnalyse) => <AnalyseDisplay analyse={analyse} />}
      render404={<AnalyseForm />}
      renderLoading={loading}
    />
  );
  // if (analyseReq.isLoading) return <LogoLoader />;
  // if (analyseReq.isError) {
  //   if (analyseReq.error.response?.status === 404)
  //     return <div>Affichage du formulaire</div>;
  //   return <ErrorHandler error={analyseReq.error} />;
  // }
}
