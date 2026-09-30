import client from "../api/client";

const getActors = async () => {
  const request = await client.get("api/ReferenceData/actors");
  return request.data;
};

const getActions = async () => {
  const request = await client.get("api/ReferenceData/actions");
  return request.data;
};

const getConstraints = async () => {
  const request = await client.get("api/ReferenceData/constraints");
  return request.data;
};

const getNature = async () => {
  const request = await client.get("api/ReferenceData/nature");
  return request.data;
};

const getConcerning = async () => {
  const request = await client.get("api/ReferenceData/concerning");
  return request.data;
};

export { getActions, getActors, getConcerning, getConstraints, getNature };
