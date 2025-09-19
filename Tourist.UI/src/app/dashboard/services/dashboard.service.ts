import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TouristPlace } from '../models/tourist-place.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiBase = `${environment.apiUrl}/Dashboard`;

  constructor(private http: HttpClient) {}

  getPlaces(): Observable<TouristPlace[]> {
    return this.http.get<TouristPlace[]>(`${this.apiBase}/getalltouristplaces`);
  }

  getPlaceById(id: string): Observable<TouristPlace> {
    return this.http.get<TouristPlace>(`${this.apiBase}/gettouristplacebyid/${id}`);
  }

  addPlace(place: TouristPlace): Observable<TouristPlace> {
    return this.http.post<TouristPlace>(`${this.apiBase}/addtouristplace`, place);
  }

  updatePlace(id: string, place: TouristPlace): Observable<TouristPlace> {
    return this.http.put<TouristPlace>(`${this.apiBase}/updatetouristplace/${id}`, place);
  }

  deletePlace(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBase}/deletetouristplace/${id}`);
  }
}
