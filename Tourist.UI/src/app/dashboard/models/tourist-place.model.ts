export interface TouristPlace {
  id?: string;
  country: string;
  state: string;
  city: string; // rename to district in UI if needed
  placeName: string;
  description: string;
  location: string;
  imageUrl: string;
  entryFee: number;
  createdBy?: string; // email or user identifier of creator
}
