using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace ModelEINP.Model;

public class Bison : AbstractAnimal {

    [ActiveConstructor]
    public Bison() {
    }
    
    [ActiveConstructor]
    public Bison(
        LandscapeLayer landscapeLayer, 
        Perimeter perimeter,
        VegetationLayer vegetationLayer,
        VectorWaterLayer waterLayer,
        RasterWaterLayer rasterWaterLayer,
        Guid id,
        AnimalType animalType,
        bool isLeading,
        int herdId,
        double latitude, 
        double longitude,
        Position position) : 
        base(landscapeLayer, 
        perimeter,
        vegetationLayer,
        waterLayer,
        rasterWaterLayer,
        id,
        animalType,
        isLeading,
        herdId,
        latitude, 
        longitude,
        position) { 
    }
    
    #region Properties and Fields
    
    public override double Hydration { get; set; } = MaxHydration;
    public override double Satiety { get; set; } = MaxSatiety;
    public override Position Position { get; set; }
    public override Position Target { get; set; }
    [PropertyDescription(Name = "Latitude")]
    public override double Latitude { get; set; }
    [PropertyDescription(Name = "Longitude")]
    public override double Longitude { get; set; }
    //Chance for a female animal to become pregnant per year
    public int ChanceForPregnancy = 10;


    
    protected string BisonType;
        
    #endregion
    
    #region Constants
    private readonly Dictionary<AnimalLifePeriod, double> _starvationRate = new()
    {
        /*
         * DailyFood / 16 is the gross starvation rate
         * but MaxSatiety = 100 != total food need per day
         * so the rate has to be adjusted
         * Total food need per day : 100 = (Total food need per day / 24) : adjusted rate
         */
        { AnimalLifePeriod.Calf, MaxSatiety * DailyFoodCalf / 16 / DailyFoodAdult }, 
        { AnimalLifePeriod.Adolescent, MaxSatiety * DailyFoodAdolescent / 16 / DailyFoodAdult }, 
        { AnimalLifePeriod.Adult, MaxSatiety * DailyFoodAdult / 16 / DailyFoodAdult }  
    };
    
    private readonly Dictionary<AnimalLifePeriod, double> _dehydrationRate =
        new()
        {
            /*
             * DailyWater / 24 is the gross starvation rate
             * but MaxHydration = 100 != total food need per day
             * so the rate has to be adjusted
             * Total water need per day : 100 = (Total water need per day / 24) : adjusted rate
             */
            { AnimalLifePeriod.Calf, MaxHydration * DailyWaterCalf / 24 / DailyWaterAdult },
            { AnimalLifePeriod.Adolescent, MaxHydration * DailyWaterAdolescent / 24 / DailyWaterAdult}, 
            { AnimalLifePeriod.Adult, MaxHydration * DailyWaterAdult / 24 / DailyWaterAdult}
        };
    
    [PropertyDescription]
    public static double DailyFoodAdult { get; set; }
    [PropertyDescription]
    public static double DailyFoodCalf { get; set; } 
    [PropertyDescription]
    public static double DailyFoodAdolescent { get; set; }
    
    //total need of water per day in liters   
    [PropertyDescription]
    public static double DailyWaterAdult { get; set; }
    [PropertyDescription]
    public static double DailyWaterCalf { get; set; }
    [PropertyDescription]
    public static double DailyWaterAdolescent { get; set; }
    #endregion
    
    public override void Tick() { 
        
        _hoursLived++;
        if (_hoursLived % 1 == 0 && _pregnant) {
            if (_pregnancyDuration < 8) {
                _pregnancyDuration++;
            }
            else {
                _pregnancyDuration = 0;
                _landscapeLayer.SpawnBison(_landscapeLayer, _perimeter, _vegetationLayer, _vectorWaterLayer, _rasterWaterLayer,
                    AnimalType.BisonCalf, false, _herdId, Latitude, Longitude, Position);
            }
        }
        if (_hoursLived == 2)
        {
            YearlyRoutine();
        }
        if (!IsAlive) return;
       
        if (Satiety < 40) {
            LookForFoodAndEat();
        }
        else if (Hydration < 40) {
            LookForWaterAndDrink();
        }
        else {
            DoRandomWalk(10);
        }
        UpdateState();
    }
    protected override void UpdateState()
    {
        int currentHour;
        if (_landscapeLayer.Context.CurrentTimePoint != null)
            currentHour = _landscapeLayer.Context.CurrentTimePoint.Value.Hour;
        else
            throw new NullReferenceException();
        

        if (currentHour is >= 21 and <= 23 || currentHour is >= 0 and <= 4 ) {   
            BurnSatiety(_starvationRate[_LifePeriod] / 4); //less food is consumed while sleeping
            Dehydrate(_dehydrationRate[_LifePeriod]/2);           //less water is consumed at night
        }
        else
        {
            BurnSatiety(_starvationRate[_LifePeriod]);
            Dehydrate(_dehydrationRate[_LifePeriod]);
        }
    }
    
    public override void YearlyRoutine() {
        _hoursLived = 0;
        Age++;

        //decide sex once the bison reaches the adult stage
        var newLifePeriod = GetAnimalLifePeriodFromAge(Age);
        if (newLifePeriod != _LifePeriod) {
            if (newLifePeriod == AnimalLifePeriod.Adult) {
                //50:50 chance of being male or female
                if (_random.Next(2) == 0)
                    _animalType = AnimalType.BisonBull;
                else
                    _animalType = AnimalType.BisonCow;
            }
            _LifePeriod = newLifePeriod;
        }
        
        //max age 25
        if (Age > 15)
        {
            _chanceOfDeath = (Age - 15) * 10;
            var rnd = _random.Next(0, 100);
            if (rnd >= _chanceOfDeath) return;
            Die(MattersOfDeath.Age);
            return;
        }

        //check for possible reproduction
        if (!(Age >= _reproductionYears[0] && Age <= _reproductionYears[1])) return;

        if (!_animalType.Equals(AnimalType.BisonCow)) return;

        if (_LifePeriod == AnimalLifePeriod.Adult && _random.Next(100) < ChanceForPregnancy-1) {
            _pregnant = true;
        }
    }
    
    public override AnimalLifePeriod GetAnimalLifePeriodFromAge(int age)
    {
        if (age < 1) return AnimalLifePeriod.Calf;
        return age <= 2 ? AnimalLifePeriod.Adolescent : AnimalLifePeriod.Adult;
    }
    
}
