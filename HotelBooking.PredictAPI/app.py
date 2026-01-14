from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import date
import pandas as pd
import joblib
import os
from pydantic import BaseModel, Field


MODEL_PATH = os.getenv("MODEL_PATH", "artifacts/adr_model.joblib")

app = FastAPI(title="Pricing Service", version="1.0")

# Allow cross-origin requests from local frontend during development
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://localhost:3001"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

if not os.path.exists(MODEL_PATH):
    raise RuntimeError(f"Model not found: {MODEL_PATH}. Run train.py first.")

model = joblib.load(MODEL_PATH)

class PredictReq(BaseModel):
    checkInDate: date = Field(..., example="2026-03-14")
    nights: int = Field(..., gt=0, example=2)
    adults: int = Field(2, ge=1)
    children: int = Field(0, ge=0)
    hotelType: str = Field("City Hotel")
    marketSegment: str = Field("Direct")
    customerType: str = Field("Transient")
    depositType: str = Field("No Deposit")
    meal: str = Field("BB")
    isRepeatedGuest: int = Field(0, ge=0, le=1)
    specialRequests: int = Field(0, ge=0)
    leadTimeDays: int = Field(14, ge=0)

@app.get("/health")
def health_check():
    """Health check endpoint for Docker"""
    return {
        "status": "healthy",
        "service": "predict-api",
        "model_loaded": model is not None,
        "model_path": MODEL_PATH
    }

@app.post("/api/v1/pricing/predict")
def predict(req: PredictReq):
    if req.nights <= 0:
        raise HTTPException(status_code=400, detail="nights must be > 0")

    
    stays_weekend = min(2, req.nights)  # basit yaklaşım
    stays_week = max(0, req.nights - stays_weekend)

    dt = req.checkInDate

    row = {
        "hotel": req.hotelType,
        "arrival_date_month": dt.strftime("%B"),  # January, February...
        "arrival_date_week_number": int(dt.strftime("%U")),
        "arrival_date_day_of_month": dt.day,
        "meal": req.meal,
        "market_segment": req.marketSegment,
        "distribution_channel": "Direct",
        "customer_type": req.customerType,
        "deposit_type": req.depositType,
        "lead_time": req.leadTimeDays,
        "stays_in_weekend_nights": stays_weekend,
        "stays_in_week_nights": stays_week,
        "adults": req.adults,
        "children": req.children,
        "babies": 0,
        "is_repeated_guest": req.isRepeatedGuest,
        "previous_cancellations": 0,
        "booking_changes": 0,
        "required_car_parking_spaces": 0,
        "total_of_special_requests": req.specialRequests
    }

    X = pd.DataFrame([row])

    adr = float(model.predict(X)[0])
    total = adr * req.nights

    return {
        "predictedAdrPerNight": round(adr, 2),
        "predictedTotal": round(total, 2),
        "currency": "EUR"
    }
